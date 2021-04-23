using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Microsoft.Extensions.Hosting;

namespace DataFair
{
    public class EntityWriter
    {
        private static Random rnd = new Random();
        private Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();

        private readonly NpgsqlConnection StreamReadConnection;
        private readonly NpgsqlConnection ReadConnention;
        private readonly NpgsqlConnection WriteConnention;
        private readonly NpgsqlCommand CheckUser;
        private readonly NpgsqlCommand CheckChat;



        internal readonly string ConnectionString;
        private readonly Thread UsersWritingThread;
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public EntityWriter(string connectionString)
        {
            this.ConnectionString = connectionString;
            ReadConnention = new NpgsqlConnection(ConnectionString);
            ReadConnention.Open();
            WriteConnention = new NpgsqlConnection(ConnectionString);
            WriteConnention.Open();
            StreamReadConnection = new NpgsqlConnection(ConnectionString);
            StreamReadConnection.Open();
            UsersWritingThread = new Thread(new ParameterizedThreadStart(EntitiesWriter));
            UsersWritingThread.Start(CancellationTokenSource.Token);

            CheckUser = ReadConnention.CreateCommand();
            CheckUser.CommandType = System.Data.CommandType.StoredProcedure;
            CheckUser.CommandText = "check_user";
            CheckUser.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            CheckChat = ReadConnention.CreateCommand();
            CheckChat.CommandType = System.Data.CommandType.StoredProcedure;
            CheckChat.CommandText = "check_chat";
            CheckChat.Parameters.Add(new NpgsqlParameter("chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));

        }
        private static void WriteSingleUser(NpgsqlCommand command, Entity entity)
        {
            command.Parameters["_user_id"].Value = entity.Id;
            command.Parameters["sender_username"].Value = entity.Link;
            command.Parameters["sender_first_name"].Value = entity.FirstName;
            command.Parameters["sender_last_name"].Value = entity.LastName;
            command.ExecuteNonQuery();
        }
        private static void WriteSingleEntity(NpgsqlCommand command, Entity entity)
        {
            command.Parameters["_chat_id"].Value = entity.Id;
            command.Parameters["_username"].Value = entity.Link.Equals(string.Empty) ? DBNull.Value : entity.Link;
            command.Parameters["_title"].Value = entity.FirstName;
            command.Parameters["pair_chat_id"].Value = entity.PairId != 0 ? entity.PairId : DBNull.Value;
            command.Parameters["_is_group"].Value = entity.Type == EntityType.Group;
            command.Parameters["_is_channel"].Value = entity.Type == EntityType.Channel;
            command.ExecuteNonQuery();
        }
        private void EntitiesWriter(object cancellationToken)
        {
            NpgsqlConnection Connention = new NpgsqlConnection(ConnectionString);
            Connention.Open();
            NpgsqlCommand AddUserCommand = Connention.CreateCommand();
            AddUserCommand.CommandType = System.Data.CommandType.StoredProcedure;
            AddUserCommand.CommandText = "add_user"; ;
            AddUserCommand.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddUserCommand.Parameters.Add(new NpgsqlParameter("sender_username", NpgsqlTypes.NpgsqlDbType.Text));
            AddUserCommand.Parameters.Add(new NpgsqlParameter("sender_first_name", NpgsqlTypes.NpgsqlDbType.Text));
            AddUserCommand.Parameters.Add(new NpgsqlParameter("sender_last_name", NpgsqlTypes.NpgsqlDbType.Text));

            NpgsqlCommand AddChatCommand = Connention.CreateCommand();
            AddChatCommand.CommandType = System.Data.CommandType.StoredProcedure;
            AddChatCommand.CommandText = "add_chat"; ;
            AddChatCommand.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddChatCommand.Parameters.Add(new NpgsqlParameter("_title", NpgsqlTypes.NpgsqlDbType.Text));
            AddChatCommand.Parameters.Add(new NpgsqlParameter("_username", NpgsqlTypes.NpgsqlDbType.Text));
            AddChatCommand.Parameters.Add(new NpgsqlParameter("pair_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddChatCommand.Parameters.Add(new NpgsqlParameter("_is_group", NpgsqlTypes.NpgsqlDbType.Boolean));
            AddChatCommand.Parameters.Add(new NpgsqlParameter("_is_channel", NpgsqlTypes.NpgsqlDbType.Boolean));

            CancellationToken Token = (CancellationToken)cancellationToken;
            while (!Token.IsCancellationRequested)
            {
                try
                {
                    while (!entities.IsEmpty)
                    {
                        using (NpgsqlTransaction transaction = Connention.BeginTransaction())
                        {
                            try
                            {
                                int count = 0;
                                for (int i = 0; i < 10000 && !entities.IsEmpty; i++)
                                {
                                    if (entities.TryDequeue(out Entity entity))
                                    {
                                        switch (entity.Type)
                                        {
                                            case EntityType.User:
                                                WriteSingleUser(AddUserCommand, entity);
                                                break;
                                            case EntityType.Group:
                                            case EntityType.Channel:
                                                WriteSingleEntity(AddChatCommand, entity);
                                                break;

                                        }

                                        count++;
                                    }
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                logger.Error(ex, "Error while writing entities!");
                            }
                        }
                    }
                    Thread.Sleep(300);
                }
                catch (Exception ex)
                {
                }
            }

            Connention.Close();
            Connention.Dispose();
        }
        public void PutEntity(Entity entity)
        {
            entities.Enqueue(entity);
        }
        public void Stop()
        {
            ReadConnention.Dispose();
            WriteConnention.Dispose();
            CancellationTokenSource.Cancel();
        }
        public int GetEntitiesNumberInQueue()
        {
            return entities.Count;
        }
    }
    public class MessageWriter
    {
        private static Random rnd = new Random();
        private Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();

        private readonly NpgsqlConnection StreamReadConnection;
        private readonly NpgsqlConnection ReadConnention;
        private readonly NpgsqlConnection WriteConnention;
        private readonly NpgsqlCommand CheckUser;
        private readonly NpgsqlCommand CheckChat;
        


        private readonly object ReadLocker = new object();
        internal readonly string ConnectionString;
        private readonly Thread MessagesWritingThread;
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public MessageWriter(string connectionString)
        {
            this.ConnectionString = connectionString;
            ReadConnention = new NpgsqlConnection(ConnectionString);
            ReadConnention.Open();
            WriteConnention = new NpgsqlConnection(ConnectionString);
            WriteConnention.Open();
            StreamReadConnection = new NpgsqlConnection(ConnectionString);
            StreamReadConnection.Open();
            MessagesWritingThread = new Thread(new ParameterizedThreadStart(MessagesWriter));
            MessagesWritingThread.Start(CancellationTokenSource.Token);

            CheckUser = ReadConnention.CreateCommand();
            CheckUser.CommandType = System.Data.CommandType.StoredProcedure;
            CheckUser.CommandText = "check_user";
            CheckUser.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            CheckChat = ReadConnention.CreateCommand();
            CheckChat.CommandType = System.Data.CommandType.StoredProcedure;
            CheckChat.CommandText = "check_chat";
            CheckChat.Parameters.Add(new NpgsqlParameter("chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));

        }
        private static void WriteSingleMessage(NpgsqlCommand command, Message message)
        {
            command.Parameters["_message_timestamp"].Value = message.Timestamp.ToDateTime();
            command.Parameters["_message_id"].Value = message.Id;
            command.Parameters["_chat_id"].Value = message.ChatId;
            command.Parameters["_user_id"].Value = message.FromId!=0? message.FromId :DBNull.Value;
            command.Parameters["_reply_to"].Value = message.ReplyTo != 0 ? message.ReplyTo : DBNull.Value;
            command.Parameters["_thread_start"].Value = message.ThreadStart != 0 ? message.ThreadStart : DBNull.Value;
            command.Parameters["_media_group_id"].Value = message.MediagroupId != 0 ? message.MediagroupId : DBNull.Value;
            command.Parameters["_forward_from_id"].Value = message.ForwardFromId != 0 ? message.ForwardFromId : DBNull.Value;
            command.Parameters["_forward_from_message_id"].Value = message.ForwardFromMessageId != 0 ? message.ForwardFromMessageId : DBNull.Value;
            command.Parameters["_text"].Value = message.Text;
            command.Parameters["_media"].Value = message.Media;
            command.Parameters["_formatting"].Value = message.Formating.Count==0?DBNull.Value:Newtonsoft.Json.JsonConvert.SerializeObject(message.Formating);
            command.ExecuteNonQuery();
        }
        private void MessagesWriter(object cancellationToken)
        {
            NpgsqlConnection Connention = new NpgsqlConnection(ConnectionString);
            Connention.Open();
            NpgsqlCommand AddMessageCommand = Connention.CreateCommand();
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

            CancellationToken Token = (CancellationToken)cancellationToken;
            while (!Token.IsCancellationRequested)
            {
                try
                {
                    while (!messages.IsEmpty)
                    {
                        using (NpgsqlTransaction transaction = Connention.BeginTransaction())
                        {
                            try
                            {
                                int count = 0;
                                for (int i = 0; i < 25000 && !messages.IsEmpty; i++)
                                {
                                    if (messages.TryDequeue(out Message message))
                                    {
                                        AddMessageCommand.Transaction = transaction;
                                        WriteSingleMessage(AddMessageCommand, message);
                                        count++;
                                    }
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                logger.Error(ex, "Error while writing messages!");
                            }
                        }
                    }
                    Thread.Sleep(300);
                }
                catch (Exception ex)
                {
                }
            }

            Connention.Close();
            Connention.Dispose();
        }
        public void PutMessage(Message message)
        {
            messages.Enqueue(message);
        }
        public void Stop()
        {
            ReadConnention.Dispose();
            WriteConnention.Dispose();
            CancellationTokenSource.Cancel();
        }
        public int GetMessagesNumberInQueue()
        {
            return messages.Count;
        }
    }
}
