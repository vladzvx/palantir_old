using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Common;
using System.Threading.Tasks;
using NLog;
using System.Data.Common;
using DataFair.Services.Interfaces;

namespace DataFair.Services
{
    public class MessageWriterCore : IWriterCore<Message>
    {
        public string ConnectionString => Options.ConnectionString;

        public int TrasactionSize => Options.MessageWriterTrasactiobSize;

        public DbCommand CommandCreator(DbConnection connection)
        {
            DbCommand AddMessageCommand = connection.CreateCommand();
            AddMessageCommand.CommandType = System.Data.CommandType.StoredProcedure;
            AddMessageCommand.CommandText = "add_message";
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_message_timestamp", NpgsqlTypes.NpgsqlDbType.Timestamp));
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_message_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_reply_to", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_thread_start", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_media_group_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_forward_from_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_forward_from_message_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_text", NpgsqlTypes.NpgsqlDbType.Text));
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_media", NpgsqlTypes.NpgsqlDbType.Text));
            AddMessageCommand.Parameters.Add(new NpgsqlParameter("_formatting", NpgsqlTypes.NpgsqlDbType.Text));
            return AddMessageCommand;
        }

        public void WriteSingleObject(DbCommand command, Message message)
        {
            command.Parameters["_message_timestamp"].Value = message.Timestamp.ToDateTime();
            command.Parameters["_message_id"].Value = message.Id;
            command.Parameters["_chat_id"].Value = message.ChatId;
            command.Parameters["_user_id"].Value = message.FromId != 0 ? message.FromId : DBNull.Value;
            command.Parameters["_reply_to"].Value = message.ReplyTo != 0 ? message.ReplyTo : DBNull.Value;
            command.Parameters["_thread_start"].Value = message.ThreadStart != 0 ? message.ThreadStart : DBNull.Value;
            command.Parameters["_media_group_id"].Value = message.MediagroupId != 0 ? message.MediagroupId : DBNull.Value;
            command.Parameters["_forward_from_id"].Value = message.ForwardFromId != 0 ? message.ForwardFromId : DBNull.Value;
            command.Parameters["_forward_from_message_id"].Value = message.ForwardFromMessageId != 0 ? message.ForwardFromMessageId : DBNull.Value;
            command.Parameters["_text"].Value = message.Text;
            command.Parameters["_media"].Value = message.Media;
            command.Parameters["_formatting"].Value = message.Formating.Count == 0 ? DBNull.Value : Newtonsoft.Json.JsonConvert.SerializeObject(message.Formating);
            command.ExecuteNonQuery();
        }
    }
}
