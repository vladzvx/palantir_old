using Common.Services.DataBase.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

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

    public class OrdersManager : IHostedService
    {
        public CancellationTokenSource cancellationTokenSource;
        public Thread worker;

        private readonly State state;
        private readonly ISettings settings;
        private readonly IOrdersGenerator ordersGenerator;
        private int ordersCount;
        private DateTime lastCycleRestart = DateTime.UtcNow.Date;
        private DateTime currentStateStarted = DateTime.UtcNow;
        public bool heavyOrdersDone = true;
        private readonly object locker = new object();
        private ExecutingState _executingState;
        public ExecutingState executingState
        {
            get
            {
                lock (locker)
                {
                    return _executingState;
                }
            }
            private set
            {
                lock (locker)
                {
                    _executingState = value;
                }
            }
        }
        public OrdersManager(State state, IOrdersGenerator ordersGenerator, ISettings settings)
        {
            this.state = state;
            this.settings = settings;
            this.ordersGenerator = ordersGenerator;
            ordersCount = state.CountOrders() + state.CountTargetOrders();
            worker = new Thread(new ParameterizedThreadStart(MainWork));
            cancellationTokenSource = new CancellationTokenSource();
        }


        private void MainWork(object cancellationToken)
        {
            state.ordersManager = this;
            GoToUpdates();
            if (cancellationToken is CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    TrySwitchStatus();
                    var temp = settings.OrdersManagerCheckingPeriod;
                    Thread.Sleep(temp);
                }
            }
        }

        private bool isWorkStopped()
        {
            int ordersCountOld = ordersCount;
            ordersCount = state.CountOrders() + state.CountTargetOrders();
            bool t = ordersCountOld == ordersCount;
            return t;
        }

        private void GoToUpdates()
        {
            currentStateStarted = DateTime.UtcNow;
            executingState = ExecutingState.OrdersCreation;
            state.ClearOrders();
            ordersGenerator.CreateUpdatesOrders(CancellationToken.None).Wait();
            executingState = ExecutingState.UpdatesLoading;
        }

        private void GoToHistory()
        {
            currentStateStarted = DateTime.UtcNow;
            executingState = ExecutingState.OrdersCreation;
            state.ClearOrders();
            ordersGenerator.CreateGetHistoryOrders(CancellationToken.None).Wait();
            executingState = ExecutingState.HistoryLoading;
        }

        private void GoToHeavy()
        {
            currentStateStarted = DateTime.UtcNow;
            state.ClearOrders();
            executingState = ExecutingState.OrdersCreation;
            ordersGenerator.CreateGetNewGroupsOrders(CancellationToken.None).Wait();
            ordersGenerator.CreateGetConsistenceSupportingOrders(CancellationToken.None).Wait();
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
                    state.ExecutingOrdersJournal.Clear();
                }
                else
                {
                    //switch (executingState)
                    //{
                    //    case ExecutingState.Started:
                    //        {
                    //            GoToUpdates();
                    //            break;
                    //        }
                    //    case ExecutingState.UpdatesLoading:

                    //        if (isWorkStopped())
                    //        {
                    //            if (heavyOrdersDone)
                    //            {
                    //                GoToHistory();
                    //            }
                    //            else
                    //            {
                    //                GoToHeavy();
                    //            }
                    //        }
                    //        break;
                    //    case ExecutingState.HeavyOrdersExecuting:
                    //        if (isWorkStopped())
                    //        {
                    //            heavyOrdersDone = true;
                    //            GoToUpdates();
                    //        }
                    //        break;
                    //    case ExecutingState.HistoryLoading:
                    //        if (isWorkStopped() || DateTime.UtcNow.Subtract(currentStateStarted).TotalHours > 3)
                    //        {
                    //            GoToUpdates();
                    //        }
                    //        break;
                    //    default:
                    //        break;

                    //}
                }

            }
            catch { }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            worker.Start(cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
