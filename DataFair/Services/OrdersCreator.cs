using Common;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DataFair.Services
{
    public class OrdersCreator:IHostedService
    {
        private System.Timers.Timer timer = new System.Timers.Timer(Options.OrderGenerationTimerPeriod);
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly State state;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly object sync = new object();
        public OrdersCreator(State state)
        {
            this.state = state;
            timer.Elapsed += TimerAction;
        }

        private void TimerAction(object sender,ElapsedEventArgs args)
        {
            if (state.Orders.Count==0&&Monitor.TryEnter(sync))
            {
                try
                {
                    Task.WaitAll(CreateHistoryLoadingOrders());//, CreateGetFullChannelOrders());
                }
                catch { }
                Monitor.Exit(sync);
            }
        }

        private static NpgsqlCommand CreateAndConfigureCommand(NpgsqlConnection connection, DateTime BoundDateTime, string storedProcedureName)
        {
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = storedProcedureName;
            command.Parameters.Add(new NpgsqlParameter("dt", NpgsqlTypes.NpgsqlDbType.Timestamp));
            command.Parameters["dt"].Value = BoundDateTime;
            return command;
        }

        public async Task CreateUpdateOrders()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Options.ConnectionString))
                {
                    await connection.OpenAsync(cts.Token);
                    using NpgsqlCommand command = CreateAndConfigureCommand(connection, DateTime.Now + Options.OrderGenerationTimeSpan, "get_unupdated_chats");
                    using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cts.Token);
                    while (!cts.IsCancellationRequested && await reader.ReadAsync(cts.Token))
                    {
                        try
                        {
                            long ChatId = reader.GetInt64(0);
                            long PairId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                            long Offset = reader.GetInt64(2);
                            DateTime LastUpdate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
                            bool PairChecked = reader.IsDBNull(4) ? true : reader.GetBoolean(4);
                            string Username = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            string PairUsername = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

                            if (!state.Orders.Any((order) => { return order.Id == ChatId && order.Type == OrderType.History; }))
                            {
                                Order order = new Order()
                                {
                                    Id = ChatId,
                                    Link = Username,
                                    Offset = Offset,
                                    PairId = PairId,
                                    PairLink = PairUsername,
                                    Type = OrderType.History
                                };
                                state.Orders.Enqueue(order);
                            }
                        }
                        catch (InvalidCastException ex) { logger.Warn(ex); }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while UpdateOrdersCreation");
            }

        }
        public async Task CreateHistoryLoadingOrders()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Options.ConnectionString))
                {
                    await connection.OpenAsync(cts.Token);
                    using NpgsqlCommand command = CreateAndConfigureCommand(connection, DateTime.Now - Options.OrderGenerationTimeSpan, "get_chats_for_history");
                    using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cts.Token);
                    while (!cts.IsCancellationRequested && await reader.ReadAsync(cts.Token))
                    {
                        try
                        {
                            long ChatId = reader.GetInt64(0);
                            long PairId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                            long Offset = reader.GetInt64(2);
                            DateTime LastUpdate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
                            bool PairChecked = reader.IsDBNull(4) ? true : reader.GetBoolean(4);
                            string Username = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            string PairUsername = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

                            if (!state.Orders.Any((order) => { return order.Id == ChatId && order.Type == OrderType.History; }))
                            {
                                Order order = new Order()
                                {
                                    Id = ChatId,
                                    Link = Username,
                                    Offset = Offset,
                                    PairId = PairId,
                                    PairLink = PairUsername,
                                    Type = OrderType.History
                                };
                                state.Orders.Enqueue(order);
                            }
                        }
                        catch (InvalidCastException ex) { logger.Warn(ex); }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while UpdateOrdersCreation");
            }

        }
        public async Task CreateGetFullChannelOrders(int limit=-1)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Options.ConnectionString))
                {
                    await connection.OpenAsync(cts.Token);
                    using NpgsqlCommand command = CreateAndConfigureCommand(connection, DateTime.Now, "get_unrequested_channels");
                    using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cts.Token);
                    int count = 0;
                    while (!cts.IsCancellationRequested&& await reader.ReadAsync(cts.Token))
                    {
                        if (limit>0 && count > limit) return;
                        try
                        {
                            long ChatId = reader.GetInt64(0);
                            long PairId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                            long Offset = reader.GetInt64(2);
                            DateTime LastUpdate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
                            bool PairChecked = reader.IsDBNull(4) ? true : reader.GetBoolean(4);
                            string Username = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            string PairUsername = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

                            Order order = new Order() { Id = ChatId, Link = Username, Offset = Offset, PairId = PairId, PairLink = PairUsername };
                            if (!PairChecked)
                            {
                                if (!state.Orders.Any((order) => { return order.Id == ChatId && order.Type == OrderType.GetFullChannel; }))
                                {
                                    order.Type = OrderType.GetFullChannel;
                                    state.Orders.Enqueue(order);
                                }
                            }
                            count++;

                        }
                        catch (InvalidCastException ex) { logger.Warn(ex); }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while GetFullChannelOrdersCreation");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Stop();
            cts.Cancel();
            return Task.CompletedTask;
        }
    }
}
