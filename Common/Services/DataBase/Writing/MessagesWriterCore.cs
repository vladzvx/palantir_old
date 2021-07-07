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
using System.Collections.Generic;

namespace Common.Services
{
    public class MessagesWriterCore : IWriterCore<Message>
    {
        public Task ExecuteAdditionaAcion(DbCommand command, object data, CancellationToken token)
        {
            throw new NotImplementedException();
        }
        public Task ExecuteAdditionaAcion(object data)
        {
            throw new NotImplementedException();
        }
        public DbCommand CreateMainCommand(DbConnection connection)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "add_message";
            command.Parameters.Add(new NpgsqlParameter("_message_timestamp", NpgsqlTypes.NpgsqlDbType.Timestamp));
            command.Parameters.Add(new NpgsqlParameter("_message_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_reply_to", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_thread_start", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_media_group_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_forward_from_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_forward_from_message_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("_text", NpgsqlTypes.NpgsqlDbType.Text));
            command.Parameters.Add(new NpgsqlParameter("_media", NpgsqlTypes.NpgsqlDbType.Jsonb));
            command.Parameters.Add(new NpgsqlParameter("_formatting", NpgsqlTypes.NpgsqlDbType.Jsonb));
            return command;
        }

        public async Task ExecuteWriting(DbCommand command, Message message, CancellationToken token)
        {
            //try
            //{
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

            bool has_form = Formating.ClearEmpty(message.Formating, out List<Formating> res);
            object forms = !has_form ?
                    DBNull.Value :
                    "{\"formats\":" + Newtonsoft.Json.JsonConvert.SerializeObject(res) + "}";

            command.Parameters["_formatting"].Value = forms;
                await command.ExecuteNonQueryAsync(token);
                return;
            //}
            //catch(Exception ex)
            //{
            //    int q = 0;
            //}

        }

        public Task ExecuteAdditionaAcion(DbConnection connection, object data, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
