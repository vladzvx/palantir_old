using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Common;
using System.Threading.Tasks;
using NLog;
using System.Data.Common;
using Common.Services.Interfaces;
using Common.Models;
using Common.Services.DataBase;

namespace Common.Services
{
    public class EntityWriterCore : IWriterCore<Entity>
    {
        private DbCommand Command;
        private DbCommand AdditionalCommand;
        public DbCommand CreateMainCommand(DbConnection connection)
        {
            Command = connection.CreateCommand();
            Command.CommandType = System.Data.CommandType.StoredProcedure;
            Command.CommandText = "add_chat"; ;
            Command.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            Command.Parameters.Add(new NpgsqlParameter("_title", NpgsqlTypes.NpgsqlDbType.Text));
            Command.Parameters.Add(new NpgsqlParameter("_username", NpgsqlTypes.NpgsqlDbType.Text));
            Command.Parameters.Add(new NpgsqlParameter("pair_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            Command.Parameters.Add(new NpgsqlParameter("_is_group", NpgsqlTypes.NpgsqlDbType.Boolean));
            Command.Parameters.Add(new NpgsqlParameter("_is_channel", NpgsqlTypes.NpgsqlDbType.Boolean));
            return Command;
        }

        public async Task Write(Entity entity, CancellationToken token)
        {
            Command.Parameters["_chat_id"].Value = entity.Id;
            Command.Parameters["_username"].Value = entity.Link.Equals(string.Empty) ? DBNull.Value : entity.Link;
            Command.Parameters["_title"].Value = entity.FirstName;
            Command.Parameters["pair_chat_id"].Value = entity.PairId != 0 ? entity.PairId : DBNull.Value;
            Command.Parameters["_is_group"].Value = entity.Type == EntityType.Group;
            Command.Parameters["_is_channel"].Value = entity.Type == EntityType.Channel;
            await Command.ExecuteNonQueryAsync(token);
            return;
        }

        public DbCommand CreateAdditionalCommand(DbConnection dbConnection)
        {
            AdditionalCommand = dbConnection.CreateCommand();
            AdditionalCommand.CommandType = System.Data.CommandType.StoredProcedure;
            AdditionalCommand.CommandText = "update_last_message"; ;
            AdditionalCommand.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            AdditionalCommand.Parameters.Add(new NpgsqlParameter("_last_message_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            return AdditionalCommand;
        }

        public async Task AdditionaAcion(object data)
        {
            if (data is Order order)
            {
                AdditionalCommand.Parameters["_chat_id"].Value = order.Id;
                AdditionalCommand.Parameters["_last_message_id"].Value = order.Offset;
                await Command.ExecuteNonQueryAsync();
            }
        }
    }
}


