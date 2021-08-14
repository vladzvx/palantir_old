using Common;
using Common.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Common.Services
{
    public enum ExecutingState
    {
        Started,
        OrdersCreation,
        UpdatesLoading,
        HeavyOrdersExecuting,
        HistoryLoading
    }

    public class OrdersManager :IHostedService
    {
        public CancellationTokenSource cancellationTokenSource;
        public Thread worker;

        private readonly State state;
        private readonly OrdersGenerator ordersGenerator;
        private int ordersCount;
        private DateTime lastCycleRestart=DateTime.UtcNow.Date;
        private bool heavyOrdersDone = false;
        private ExecutingState executingState { get; set; }
        public OrdersManager(State state, OrdersGenerator ordersGenerator)
        {
            this.state = state;
            this.ordersGenerator = ordersGenerator;
            ordersCount = state.CountOrders();
            worker = new Thread(new ParameterizedThreadStart(MainWork));
            cancellationTokenSource= new CancellationTokenSource();
        }


        private void MainWork(object cancellationToken)
        {
            if (cancellationToken is CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    TrySwitchStatus();
                    Thread.Sleep(1 * 60 * 1000);
                }
            }
        }

        private bool isWorkStopped()
        {
            int ordersCountOld = ordersCount;
            ordersCount = state.CountOrders();
            return ordersCountOld == ordersCount;
        }

        private void GoToUpdates()
        {
            executingState = ExecutingState.OrdersCreation;
            state.ClearOrders();
            ordersGenerator.SetOrderUnGeneratedStatus(CancellationToken.None).Wait();
            ordersGenerator.GetUpdatesOrders(CancellationToken.None).Wait();
            executingState = ExecutingState.UpdatesLoading;
        }

        private void GoToHistory()
        {
            executingState = ExecutingState.OrdersCreation;
            state.ClearOrders();
            ordersGenerator.SetOrderUnGeneratedStatus(CancellationToken.None).Wait();
            ordersGenerator.GetHistoryOrders(CancellationToken.None).Wait();
            executingState = ExecutingState.HistoryLoading;
        }

        private void GoToHeavy()
        {
            state.ClearOrders();
            executingState = ExecutingState.OrdersCreation;
            ordersGenerator.GetNewGroups(CancellationToken.None).Wait();
            ordersGenerator.GetConsistenceSupportingOrders(CancellationToken.None).Wait();
            executingState = ExecutingState.HeavyOrdersExecuting;
        }

        private void TrySwitchStatus()
        {
            try
            {
                if (DateTime.UtcNow.Subtract(lastCycleRestart).TotalHours > 24)
                {
                    lastCycleRestart = DateTime.UtcNow.Date;
                    GoToUpdates();
                    heavyOrdersDone = false;
                }
                else
                {
                    switch (executingState)
                    {
                        case ExecutingState.Started:
                            {
                                GoToUpdates();
                                break;
                            }
                        case ExecutingState.UpdatesLoading:

                            if (isWorkStopped())
                            {
                                if (heavyOrdersDone)
                                {
                                    GoToHistory();
                                }
                                else
                                {
                                    GoToHeavy();
                                }
                            }
                            break;
                        case ExecutingState.HeavyOrdersExecuting:
                            if (isWorkStopped())
                            {
                                heavyOrdersDone = true;
                                GoToUpdates();
                            }
                            break;
                        case ExecutingState.HistoryLoading:
                            if (isWorkStopped())
                            {
                                GoToUpdates();
                            }
                            break;
                        default:
                            break;

                    }
                }

            }
            catch { }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            worker.Start(cancellationTokenSource.Token);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationTokenSource.Cancel();
        }
    }
}
