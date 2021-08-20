using Common.Services.Interfaces;
using Npgsql;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public class BotMessagesWriterCore : IWriterCore<Message>
    {
        public Task ExecuteAdditionaAcion(DbCommand command, object data, CancellationToken token)
        {
            throw new NotImplementedException();
        }
        public DbCommand CreateMainCommand(DbConnection connection)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "add_message";
            command.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_message_number", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_text", NpgsqlTypes.NpgsqlDbType.Text));
            command.Parameters.Add(new NpgsqlParameter("_tg_time", NpgsqlTypes.NpgsqlDbType.Timestamp));
            return command;
        }

        public async Task ExecuteWriting(DbCommand command, Message message, CancellationToken token)
        {
            command.Parameters["_user_id"].Value = message.From.Id;
            command.Parameters["_message_number"].Value = message.MessageId;
            command.Parameters["_chat_id"].Value = message.Chat.Id;
            command.Parameters["_text"].Value = message.Text;
            command.Parameters["_tg_time"].Value = message.Date;
            await command.ExecuteNonQueryAsync(token);
            return;
        }

        public Task ExecuteAdditionaAcion(DbConnection connection, object data, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
