using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services
{
    public class OrdersGenerator : IOrdersGenerator
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly State state;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly ConnectionsFactory connectionsFactory;
        private readonly ISettings settings;
        public OrdersGenerator(State state, ConnectionsFactory connectionsFactory, ISettings settings)
        {
            this.state = state;
            this.connectionsFactory = connectionsFactory;
            this.settings = settings;
        }

        #region main methods
        public async Task CreateUpdatesOrders(CancellationToken token)
        {
            await SetOrderUnGeneratedStatus(token);
            await CreateSingleUpdatesOrders(token);
            await SetCreateSingleUpdatesOrdersStatus(token);
            await CreatePairRequestsForConsistence(token);
            await CreatePairForRequestsOrders(token);
            await SetOrderGeneratedStatusRequest(CancellationToken.None);
        }
        public async Task CreateGetNewGroupsOrders(CancellationToken token)
        {
            await SetOrderUnGeneratedStatus(token);
            await CreatePairForRequestsOrders(token);
            await SetOrderGeneratedStatusRequest(CancellationToken.None);
        }
        public Task CreateGetConsistenceSupportingOrders(CancellationToken token)
        {
            return Task.CompletedTask;
        }
        public async Task CreateGetHistoryOrders(CancellationToken token)
        {
            await SetOrderUnGeneratedStatus(token);
            await CreateSingleHistoryOrders(token);
            await SetCreateSingleHistoryOrdersStatus(token);
        }
        #endregion

        private async Task<ConcurrentQueue<Order>> CreateSingleHistoryOrders(CancellationToken token)
        {
            try
            {
                ConcurrentQueue<Order> SingleUpdatesQueue = new ConcurrentQueue<Order>();
                using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
                {
                    using NpgsqlCommand command = connection.Connection.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "select id,username,last_message_id,finders from chats where " +
                        "(has_actual_order is null or not has_actual_order) and not banned and (finders is not null and array_length(finders,1)>0);";
                    using NpgsqlDataReader reader = await command.ExecuteReaderAsync(token);
                    while (!token.IsCancellationRequested && await reader.ReadAsync(token))
                    {
                        long ChatId = reader.GetInt64(0);
                        string Username = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        long Offset = reader.IsDBNull(2) ? 1 : reader.GetInt64(2);
                        string[] Finders = reader.IsDBNull(3) ? new string[0] : (string[])reader.GetValue(3);

                        if (string.IsNullOrEmpty(Username))
                        {
                            continue;
                        }

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
        private async Task SetCreateSingleHistoryOrdersStatus(CancellationToken token)
        {
            using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
            {
                using NpgsqlCommand command = connection.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "update chats set last_time_checked = current_timestamp, has_actual_order=true where " +
                    "(has_actual_order is null or not has_actual_order) and not banned and (finders is not null and array_length(finders,1)>0);";
                await command.ExecuteNonQueryAsync(token);
            }
        }
        private async Task<ConcurrentQueue<Order>> CreateSingleUpdatesOrders(CancellationToken token)
        {
            try
            {
                ConcurrentQueue<Order> SingleUpdatesQueue = new ConcurrentQueue<Order>();
                using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
                {
                    using NpgsqlCommand command = connection.Connection.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "select id,username,last_message_id,finders from chats where (has_actual_order is null or not has_actual_order) and not banned and (finders is not null and array_length(finders,1)>0) and (is_group or pair_id_checked);";
                    using NpgsqlDataReader reader = await command.ExecuteReaderAsync(token);
                    while (!token.IsCancellationRequested && await reader.ReadAsync(token))
                    {
                        long ChatId = reader.GetInt64(0);
                        string Username = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        long Offset = reader.IsDBNull(2) ? 1 : reader.GetInt64(2);
                        string[] Finders = reader.IsDBNull(3) ? new string[0] : (string[])reader.GetValue(3);

                        if (string.IsNullOrEmpty(Username))
                        {
                            continue;
                        }

                        Order order = new Order()
                        {
                            Id = ChatId,
                            Link = Username,
                            Offset = Offset,
                            Type = OrderType.History,
                            repeatInterval = settings.UpdatesCheckingPeriod
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
        private async Task SetCreateSingleUpdatesOrdersStatus(CancellationToken token)
        {
            using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
            {
                using NpgsqlCommand command = connection.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "update chats set last_time_checked = current_timestamp, has_actual_order=true where (has_actual_order is null or not has_actual_order) and not banned and (finders is not null and array_length(finders,1)>0) and (is_group or pair_id_checked);";
                await command.ExecuteNonQueryAsync(token);
            }
        }
        public async Task CreatePairRequestsForConsistence(CancellationToken token)
        {
            try
            {
                //ConcurrentQueue<Order> PairForRequestsQueue = new ConcurrentQueue<Order>();
                using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
                {
                    using NpgsqlCommand command = connection.Connection.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "select chats.id,chats.username,chats.last_message_id,chats.finders,cpair.id,cpair.last_message_id,cpair.username from chats left join chats cpair on chats.id = cpair.pair_id where chats.is_channel and chats.pair_id_checked and(array_length(chats.finders,1) is null) and not chats.banned and chats.pair_id is not null;";
                    using NpgsqlDataReader reader = await command.ExecuteReaderAsync(token);
                    while (!token.IsCancellationRequested && await reader.ReadAsync(token))
                    {
                        long ChatId = reader.GetInt64(0);
                        long pairChatid = reader.IsDBNull(4) ? 0:reader.GetInt64(4);
                        string Username = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        string pairUsername = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                        long Offset = reader.IsDBNull(2) ? 1 : reader.GetInt64(2);
                        long PairOffset = reader.IsDBNull(5) ? 1 : reader.GetInt64(5);
                        string[] Finders = reader.IsDBNull(3) ? new string[0] : (string[])reader.GetValue(3);

                        if (string.IsNullOrEmpty(Username))
                        {
                            continue;
                        }

                        Order order = new Order()
                        {
                            Id = ChatId,
                            Link = Username,
                            Offset = Offset,
                            PairOffset=PairOffset,
                            Type = OrderType.Pair,
                        };
                        foreach (string finder in Finders)
                        {
                            order.Finders.Add(finder);
                        }
                        state.ConsistanceOrders.Enqueue(order);
                    }
                }
                //return PairForRequestsQueue;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while UpdateOrdersCreation");
                throw ex;
            }
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

                        if (string.IsNullOrEmpty(Username))
                        {
                            continue;
                        }

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
        private async Task SetOrderGeneratedStatusRequest(CancellationToken token)
        {
            using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
            {
                using NpgsqlCommand command = connection.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "update chats set last_time_checked = current_timestamp, has_actual_order=true where is_channel and not pair_id_checked and (has_actual_order is null or not has_actual_order) and not banned;";
                await command.ExecuteNonQueryAsync(token);
            }
        }
        private async Task SetOrderUnGeneratedStatus(CancellationToken token)
        {
            using (ConnectionWrapper connection = await connectionsFactory.GetConnectionAsync(token))
            {
                using NpgsqlCommand command = connection.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "update chats set has_actual_order=null;";
                await command.ExecuteNonQueryAsync(token);
            }
        }
    }
}
