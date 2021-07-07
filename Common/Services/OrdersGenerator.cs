using Common;
using Common.Services.DataBase;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services
{
    public class OrdersGenerator
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly State state;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly ConnectionsFactory connectionsFactory;
        public OrdersGenerator(State state, ConnectionsFactory connectionsFactory)
        {
            this.state = state;
            this.connectionsFactory = connectionsFactory;
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


        #region history and updates
        public async Task GetHistoryOrders(CancellationToken token)
        {
            await CreateSingleUpdatesOrders(token);
            await SetCreateSingleUpdatesOrdersStatus(token);
        }
        public async Task<ConcurrentQueue<Order>> CreateSingleUpdatesOrders(CancellationToken token)
        {
            try
            {
                ConcurrentQueue<Order> SingleUpdatesQueue = new ConcurrentQueue<Order>();
                using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
                {
                    using NpgsqlCommand command = connection.Connection.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "select id,username,last_message_id,finders from chats where (has_actual_order is null or not has_actual_order) and not banned and (finders is not null and array_length(finders,1)>0);";
                    using NpgsqlDataReader reader = await command.ExecuteReaderAsync(token);
                    while (!token.IsCancellationRequested && await reader.ReadAsync(token))
                    {
                        long ChatId = reader.GetInt64(0);
                        string Username = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        long Offset = reader.IsDBNull(2) ? 1 : reader.GetInt64(2);
                        string[] Finders = reader.IsDBNull(3) ? new string[0] : (string[])reader.GetValue(3);

                        if (string.IsNullOrEmpty(Username)) continue;
                        Order order = new Order()
                        {
                            Id = ChatId,
                            Link = Username,
                            Offset = Offset,
                            Type = OrderType.History,
                        };
                        foreach (string finder in Finders)
                        {
                            order.Finders.Add(finder);
                        }
                        state.AddOrder(order);
                        //state.Orders.Enqueue(order);

                    }
                }
                return SingleUpdatesQueue;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while UpdateOrdersCreation");
                throw ex;
            }
        }
        public async Task SetCreateSingleUpdatesOrdersStatus(CancellationToken token)
        {
            using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
            {
                using NpgsqlCommand command = connection.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "update chats set last_time_checked = current_timestamp, has_actual_order=true where (has_actual_order is null or not has_actual_order) and not banned and (finders is not null and array_length(finders,1)>0);";
                await command.ExecuteNonQueryAsync(token);
            }
        }
        #endregion

        #region поиск новых групп

        public async Task GetNewGroupsOrders(CancellationToken token)
        {
            await CreatePairForRequestsOrders(token);
            await SetOrderGeneratedStatusRequest(CancellationToken.None);
        }

        public async Task<ConcurrentQueue<Order>> CreatePairForRequestsOrders(CancellationToken token)
        {
            try
            {
                ConcurrentQueue<Order> PairForRequestsQueue = new ConcurrentQueue<Order>();
                using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
                {
                    using NpgsqlCommand command = connection.Connection.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "select id,username,last_message_id,finders from chats where is_channel and not pair_id_checked and (has_actual_order is null or not has_actual_order) and not banned;";
                    using NpgsqlDataReader reader = await command.ExecuteReaderAsync(token);
                    while (!token.IsCancellationRequested && await reader.ReadAsync(token))
                    {
                        long ChatId = reader.GetInt64(0);
                        string Username = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        long Offset = reader.IsDBNull(2) ? 1 : reader.GetInt64(2);

                        if (string.IsNullOrEmpty(Username)) continue;
                        Order order = new Order()
                        {
                            Id = ChatId,
                            Link = Username,
                            Offset = Offset,
                            Type = OrderType.Pair,
                        };
                        state.Orders.Enqueue(order);
                    }
                }
                return PairForRequestsQueue;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while UpdateOrdersCreation");
                throw ex;
            }
        }
        public async Task SetOrderGeneratedStatusRequest(CancellationToken token)
        {
            using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
            {
                using NpgsqlCommand command = connection.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "update chats set last_time_checked = current_timestamp, has_actual_order=true where is_channel and not pair_id_checked and (has_actual_order is null or not has_actual_order) and not banned;";
                await command.ExecuteNonQueryAsync(token);
            }
        }
        #endregion

        #region восстановление старых групп
        public async Task<ConcurrentQueue<Order>> RestoreLostGroups(CancellationToken token)
        {
            try
            {
                ConcurrentQueue<Order> PairQueue = new ConcurrentQueue<Order>();
                using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
                {
                    using NpgsqlCommand command = connection.Connection.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "select c1.id,c1.pair_id,c1.username,c2.username,c1.last_message_id,c2.last_message_id,c2.finders from chats c1 inner join chats c2 on c1.id = c2.pair_id where ((c1.has_actual_order is null or not c1.has_actual_order) and (c2.has_actual_order is null or not c2.has_actual_order)) and not c1.banned and c2.is_group and c2.finders is null;";
                    using NpgsqlDataReader reader = await command.ExecuteReaderAsync(token);
                    while (!token.IsCancellationRequested && await reader.ReadAsync(token))
                    {
                        long ChatId = reader.GetInt64(0);
                        long PairId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                        string Username = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                        string PairUsername = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);

                        long Offset = reader.IsDBNull(4) ? 1 : reader.GetInt64(4);
                        long PairOffset = reader.IsDBNull(5) ? 1 : reader.GetInt64(5);
                        string[] Finders = reader.IsDBNull(6) ? new string[0] : (string[])reader.GetValue(6);

                        if (string.IsNullOrEmpty(Username)) continue;
                        Order order = new Order()
                        {
                            Id = ChatId,
                            Link = Username,
                            Offset = Offset,
                            PairOffset = PairOffset,
                            PairId = PairId,
                            PairLink = PairUsername,
                            Type = OrderType.Pair,

                        };
                        foreach (string finder in Finders)
                        {
                            order.Finders.Add(finder);
                        }
                        state.Orders.Enqueue(order);
                    }
                }
                return PairQueue;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while UpdateOrdersCreation");
                throw ex;
            }
        }

        public async Task RestoreLostGroupsSetStatus(CancellationToken token)
        {
            using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
            {
                using NpgsqlCommand command = connection.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "update chats set last_time_checked = current_timestamp, has_actual_order=true where is_group and finders is null;";
                await command.ExecuteNonQueryAsync(token);
            }
        }
        #endregion


        //#region trash



        //public async Task SetOrderGeneratedStatusRequest22(CancellationToken token)
        //{
        //    using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
        //    {
        //        using NpgsqlCommand command = connection.Connection.CreateCommand();
        //        command.CommandType = System.Data.CommandType.Text;
        //        command.CommandText = "update chats set last_time_checked = current_timestamp, has_actual_order=true where is_channel and not pair_id_checked and (has_actual_order is null or not has_actual_order) and not banned;";
        //        await command.ExecuteNonQueryAsync(token);
        //    }
        //}

        public async Task SetOrderUnGeneratedStatus(CancellationToken token)
        {
            using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
            {
                using NpgsqlCommand command = connection.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "update chats set has_actual_order=null;";
                await command.ExecuteNonQueryAsync(token);
            }
        }

        //public async Task CreateHistoryLoadingOrders(int limit = -1)
        //{
        //    try
        //    {
        //        using (NpgsqlConnection connection = new NpgsqlConnection(Options.ConnectionString))
        //        {
        //            await connection.OpenAsync(cts.Token);
        //            using NpgsqlCommand command = CreateAndConfigureCommand(connection, DateTime.Now - Options.OrderGenerationTimeSpan, "get_chats_for_history");
        //            using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cts.Token);
        //            int count = 0;
        //            while (!cts.IsCancellationRequested && await reader.ReadAsync(cts.Token))
        //            {
        //                try
        //                {
        //                    if (limit > 0 && count > limit) return;
        //                    long ChatId = reader.GetInt64(0);
        //                    long PairId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
        //                    long Offset = reader.GetInt64(2);
        //                    DateTime LastUpdate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
        //                    bool PairChecked = reader.IsDBNull(4) ? true : reader.GetBoolean(4);
        //                    string Username = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
        //                    string PairUsername = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

        //                    if (!state.Orders.Any((order) => { return order.Id == ChatId && order.Type == OrderType.History; }))
        //                    {
        //                        Order order = new Order()
        //                        {
        //                            Id = ChatId,
        //                            Link = Username,
        //                            Offset = Offset,
        //                            PairId = PairId,
        //                            PairLink = PairUsername,
        //                            Type = OrderType.History
        //                        };
        //                        state.Orders.Enqueue(order);
        //                        count++;
        //                    }
        //                }
        //                catch (InvalidCastException ex) { logger.Warn(ex); }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Error while UpdateOrdersCreation");
        //    }

        //}
        //public async Task CreateGroupHistoryLoadingOrders()
        //{
        //    try
        //    {
        //        using (NpgsqlConnection connection = new NpgsqlConnection(Options.ConnectionString))
        //        {
        //            await connection.OpenAsync(cts.Token);
        //            using NpgsqlCommand command = CreateAndConfigureCommand(connection, DateTime.Now - Options.OrderGenerationTimeSpan, "get_groups_for_history");
        //            using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cts.Token);
        //            while (!cts.IsCancellationRequested && await reader.ReadAsync(cts.Token))
        //            {
        //                try
        //                {
        //                    long ChatId = reader.GetInt64(0);
        //                    long PairId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
        //                    long Offset = reader.GetInt64(2);
        //                    DateTime LastUpdate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
        //                    bool PairChecked = reader.IsDBNull(4) ? true : reader.GetBoolean(4);
        //                    string Username = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
        //                    string PairUsername = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
        //                    Order order = new Order()
        //                    {
        //                        Id = ChatId,
        //                        Link = Username,
        //                        Offset = Offset,
        //                        PairId = PairId,
        //                        PairLink = PairUsername,
        //                        Type = OrderType.History
        //                    };
        //                    state.MaxPriorityOrders.Enqueue(order);
        //                }
        //                catch (InvalidCastException ex) { logger.Warn(ex); }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Error while UpdateOrdersCreation");
        //    }

        //}
        //public async Task CreateGetFullChannelOrders(int limit = -1)
        //{
        //    try
        //    {
        //        using (NpgsqlConnection connection = new NpgsqlConnection(Options.ConnectionString))
        //        {
        //            await connection.OpenAsync(cts.Token);
        //            using NpgsqlCommand command = CreateAndConfigureCommand(connection, DateTime.Now, "get_unrequested_channels");
        //            using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cts.Token);
        //            int count = 0;
        //            while (!cts.IsCancellationRequested && await reader.ReadAsync(cts.Token))
        //            {
        //                if (limit > 0 && count > limit) return;
        //                try
        //                {
        //                    long ChatId = reader.GetInt64(0);
        //                    long PairId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
        //                    long Offset = reader.GetInt64(2);
        //                    DateTime LastUpdate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
        //                    bool PairChecked = reader.IsDBNull(4) ? true : reader.GetBoolean(4);
        //                    string Username = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
        //                    string PairUsername = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

        //                    Order order = new Order() { Id = ChatId, Link = Username, Offset = Offset, PairId = PairId, PairLink = PairUsername, Time = 30 };
        //                    if (!PairChecked)
        //                    {
        //                        if (!state.MaxPriorityOrders.Any((order) => { return order.Id == ChatId && order.Type == OrderType.GetFullChannel; }))
        //                        {
        //                            order.Type = OrderType.GetFullChannel;
        //                            state.MaxPriorityOrders.Enqueue(order);
        //                            count++;
        //                        }
        //                    }
        //                }
        //                catch (InvalidCastException ex) { logger.Warn(ex); }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Error while GetFullChannelOrdersCreation");
        //    }
        //}

        //#endregion



    }
}
