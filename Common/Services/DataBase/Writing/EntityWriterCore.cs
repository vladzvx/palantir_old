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
        public DbCommand CreateMainCommand(DbConnection connection)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "add_chat"; ;
            command.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_title", NpgsqlTypes.NpgsqlDbType.Text));
            command.Parameters.Add(new NpgsqlParameter("_username", NpgsqlTypes.NpgsqlDbType.Text));
            command.Parameters.Add(new NpgsqlParameter("pair_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_is_group", NpgsqlTypes.NpgsqlDbType.Boolean));
            command.Parameters.Add(new NpgsqlParameter("_is_channel", NpgsqlTypes.NpgsqlDbType.Boolean));
            return command;
        }

        public async Task ExecuteWriting(DbCommand command, Entity entity, CancellationToken token)
        {
            command.Parameters["_chat_id"].Value = entity.Id;
            command.Parameters["_username"].Value = entity.Link.Equals(string.Empty) ? DBNull.Value : entity.Link;
            command.Parameters["_title"].Value = entity.FirstName;
            command.Parameters["pair_chat_id"].Value = entity.PairId != 0 ? entity.PairId : DBNull.Value;
            command.Parameters["_is_group"].Value = entity.Type == EntityType.Group;
            command.Parameters["_is_channel"].Value = entity.Type == EntityType.Channel;
            await command.ExecuteNonQueryAsync(token);
        }

        public DbCommand CreateAdditionalCommand(DbConnection dbConnection)
        {
            DbCommand additionalCommand = dbConnection.CreateCommand();
            additionalCommand.CommandType = System.Data.CommandType.StoredProcedure;
            additionalCommand.CommandText = "update_last_message"; ;
            additionalCommand.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            additionalCommand.Parameters.Add(new NpgsqlParameter("_last_message_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            return additionalCommand;
        }

        public async Task ExecuteAdditionaAcion(DbCommand additionalCommand, object data, CancellationToken token)
        {
            if (data is Order order)
            {
                additionalCommand.Parameters["_chat_id"].Value = order.Id;
                additionalCommand.Parameters["_last_message_id"].Value = order.Offset;
                await additionalCommand.ExecuteNonQueryAsync(token);
            }
        }
    }
}


