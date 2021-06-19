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
    public class MessagesWriterCore : IWriterCore<Message>
    {
        private DbCommand Command;

        public Task AdditionaAcion(object data)
        {
            throw new NotImplementedException();
        }

        public DbCommand CreateMainCommand(DbConnection connection)
        {
            Command = connection.CreateCommand();
            Command.CommandType = System.Data.CommandType.StoredProcedure;
            Command.CommandText = "add_message";
            Command.Parameters.Add(new NpgsqlParameter("_message_timestamp", NpgsqlTypes.NpgsqlDbType.Timestamp));
            Command.Parameters.Add(new NpgsqlParameter("_message_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            Command.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            Command.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            Command.Parameters.Add(new NpgsqlParameter("_reply_to", NpgsqlTypes.NpgsqlDbType.Bigint));
            Command.Parameters.Add(new NpgsqlParameter("_thread_start", NpgsqlTypes.NpgsqlDbType.Bigint));
            Command.Parameters.Add(new NpgsqlParameter("_media_group_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            Command.Parameters.Add(new NpgsqlParameter("_forward_from_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            Command.Parameters.Add(new NpgsqlParameter("_forward_from_message_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            Command.Parameters.Add(new NpgsqlParameter("_text", NpgsqlTypes.NpgsqlDbType.Text));
            Command.Parameters.Add(new NpgsqlParameter("_media", NpgsqlTypes.NpgsqlDbType.Jsonb));
            Command.Parameters.Add(new NpgsqlParameter("_formatting", NpgsqlTypes.NpgsqlDbType.Jsonb));
            return Command;
        }

        public async Task Write(Message message, CancellationToken token)
        {
                DbCommand command = Command;
                //command.Transaction = transaction;
                command.Parameters["_message_timestamp"].Value = message.Timestamp.ToDateTime();
                command.Parameters["_message_id"].Value = message.Id;
                command.Parameters["_chat_id"].Value = message.ChatId;
                command.Parameters["_user_id"].Value = message.FromId != 0 ? message.FromId : DBNull.Value;
                command.Parameters["_reply_to"].Value = message.ReplyTo != 0 ? message.ReplyTo : DBNull.Value;
                command.Parameters["_thread_start"].Value = message.ThreadStart != 0 ? message.ThreadStart : DBNull.Value;
                command.Parameters["_media_group_id"].Value = message.MediagroupId != 0 ? message.MediagroupId : DBNull.Value;
                command.Parameters["_forward_from_id"].Value = message.ForwardFromId != 0 ? message.ForwardFromId : DBNull.Value;
                command.Parameters["_forward_from_message_id"].Value = message.ForwardFromMessageId != 0 ? message.ForwardFromMessageId : DBNull.Value;
                command.Parameters["_text"].Value = !string.IsNullOrEmpty(message.Text) ? message.Text : DBNull.Value;
                command.Parameters["_media"].Value = string.IsNullOrEmpty(message.Media) ? DBNull.Value : message.Media;
                command.Parameters["_formatting"].Value = message.Formating.Count == 0 || Formating.IsEmpty(message.Formating) ?
                    DBNull.Value :
                    "{\"formats\":" + Newtonsoft.Json.JsonConvert.SerializeObject(message.Formating) + "}";
                await command.ExecuteNonQueryAsync(token);
                return;
        }
    }
}
