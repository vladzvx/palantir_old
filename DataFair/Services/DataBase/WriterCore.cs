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
    public class WriterCore : IWriterCore
    {
        private DbCommand AddMessageCommand;
        private DbCommand AddChatCommand;
        private DbCommand BanChatCommand;
        private DbCommand AddUserCommand;
        public string ConnectionString => Options.ConnectionString;

        public int TrasactionSize => Options.WriterTransactionSize;

        public TimeSpan ReconnerctionPause => Options.ReconnerctionPause;

        public double StartWritingInterval => Options.StartWritingInterval;

        public DbCommand CreateCommand(DbConnection connection, Type dataType)
        {
            if (dataType == typeof(Message))
            {
                AddMessageCommand = connection.CreateCommand();
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
                AddMessageCommand.Parameters.Add(new NpgsqlParameter("_media", NpgsqlTypes.NpgsqlDbType.Jsonb));
                AddMessageCommand.Parameters.Add(new NpgsqlParameter("_formatting", NpgsqlTypes.NpgsqlDbType.Jsonb));
                return AddMessageCommand;
            }
            else if (dataType == typeof(User))
            {
                AddUserCommand = connection.CreateCommand();
                AddUserCommand.CommandType = System.Data.CommandType.StoredProcedure;
                AddUserCommand.CommandText = "add_user"; ;
                AddUserCommand.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
                AddUserCommand.Parameters.Add(new NpgsqlParameter("sender_username", NpgsqlTypes.NpgsqlDbType.Text));
                AddUserCommand.Parameters.Add(new NpgsqlParameter("sender_first_name", NpgsqlTypes.NpgsqlDbType.Text));
                AddUserCommand.Parameters.Add(new NpgsqlParameter("sender_last_name", NpgsqlTypes.NpgsqlDbType.Text));
                return AddUserCommand;
            }
            else if (dataType == typeof(Chat))
            {
                AddChatCommand = connection.CreateCommand();
                AddChatCommand.CommandType = System.Data.CommandType.StoredProcedure;
                AddChatCommand.CommandText = "add_chat"; ;
                AddChatCommand.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
                AddChatCommand.Parameters.Add(new NpgsqlParameter("_title", NpgsqlTypes.NpgsqlDbType.Text));
                AddChatCommand.Parameters.Add(new NpgsqlParameter("_username", NpgsqlTypes.NpgsqlDbType.Text));
                AddChatCommand.Parameters.Add(new NpgsqlParameter("pair_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
                AddChatCommand.Parameters.Add(new NpgsqlParameter("_is_group", NpgsqlTypes.NpgsqlDbType.Boolean));
                AddChatCommand.Parameters.Add(new NpgsqlParameter("_is_channel", NpgsqlTypes.NpgsqlDbType.Boolean));
                return AddChatCommand;
            }
            else if (dataType == typeof(Ban))
            {
                BanChatCommand = connection.CreateCommand();
                BanChatCommand.CommandType = System.Data.CommandType.Text;
                BanChatCommand.CommandText = "update chats set banned = true where id = @_id"; ;
                BanChatCommand.Parameters.Add(new NpgsqlParameter("_id", NpgsqlTypes.NpgsqlDbType.Bigint));
                return BanChatCommand;
            }
            else throw new InvalidCastException();
        }

        public void WriteSingleObject(object data,DbTransaction transaction)
        {
            Message message = data as Message;
            if (message != null && AddMessageCommand!=null)
            {
                try
                {
                    DbCommand command = AddMessageCommand;
                    command.Transaction = transaction;
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
                    command.Parameters["_media"].Value = string.IsNullOrEmpty(message.Media)?DBNull.Value: message.Media;
                    command.Parameters["_formatting"].Value = message.Formating.Count == 0 || Formating.IsEmpty(message.Formating) ? 
                        DBNull.Value: 
                        "{\"formats\":"+ Newtonsoft.Json.JsonConvert.SerializeObject(message.Formating) + "}";
                    command.ExecuteNonQuery();
                    return;
                }
                catch (Exception ex)
                {
                    int q = 0;
                }

            }

            Chat chat = data as Chat;
            if (chat != null&&AddChatCommand!=null)
            {
                DbCommand command = AddChatCommand;
                command.Transaction = transaction;
                command.Parameters["_chat_id"].Value = chat.Entity.Id;
                command.Parameters["_username"].Value = chat.Entity.Link.Equals(string.Empty) ? DBNull.Value : chat.Entity.Link;
                command.Parameters["_title"].Value = chat.Entity.FirstName;
                command.Parameters["pair_chat_id"].Value = chat.Entity.PairId != 0 ? chat.Entity.PairId : DBNull.Value;
                command.Parameters["_is_group"].Value = chat.Entity.Type == EntityType.Group;
                command.Parameters["_is_channel"].Value = chat.Entity.Type == EntityType.Channel;
                command.ExecuteNonQuery();
                return;
            }

            User user = data as User;
            if (user != null&&AddUserCommand!=null)
            {
                DbCommand command = AddUserCommand;
                command.Transaction = transaction;
                command.Parameters["_user_id"].Value = user.Entity.Id;
                command.Parameters["sender_username"].Value = user.Entity.Link;
                command.Parameters["sender_first_name"].Value = user.Entity.FirstName;
                command.Parameters["sender_last_name"].Value = user.Entity.LastName;
                command.ExecuteNonQuery();
                return;

            }

            Ban ban = data as Ban;
            if (ban != null && BanChatCommand != null)
            {
                DbCommand command = BanChatCommand;
                command.Transaction = transaction;
                command.Parameters["_id"].Value = ban.Entity.Id;
                command.ExecuteNonQuery();
                return;
            }
        }
    }
}
