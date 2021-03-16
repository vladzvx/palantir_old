using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Common;

namespace DataFair
{
    public class DBWorker
    {
        private readonly ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();
        private readonly ConcurrentQueue<Entity> users = new ConcurrentQueue<Entity>();

        private readonly NpgsqlConnection ReadConnention;
        private readonly NpgsqlConnection WriteConnention;

        private readonly string ConnectionString;
        private readonly Thread MessagesWritingThread;
        private readonly Thread UsersWritingThread;
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public DBWorker(string connectionString)
        {
            this.ConnectionString = connectionString;
            ReadConnention = new NpgsqlConnection(ConnectionString);
            ReadConnention.Open();
            WriteConnention = new NpgsqlConnection(ConnectionString);
            WriteConnention.Open();
            MessagesWritingThread = new Thread(new ParameterizedThreadStart(MessagesWriter));
            MessagesWritingThread.Start(CancellationTokenSource.Token);
            UsersWritingThread = new Thread(new ParameterizedThreadStart(UsersWriter));
            UsersWritingThread.Start(CancellationTokenSource.Token);
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
            command.Parameters["_media"].Value = DBNull.Value;// essage.Media;
            command.Parameters["_formatting"].Value = DBNull.Value;// message.Formating;
            command.ExecuteNonQuery();
        }

        private static void WriteSingleUser(NpgsqlCommand command, Entity entity)
        {
            command.Parameters["_user_id"].Value = entity.Id;
            command.Parameters["sender_username"].Value = entity.Link;
            command.Parameters["sender_first_name"].Value = entity.FirstName;
            command.Parameters["sender_last_name"].Value = entity.LastName;
            command.ExecuteNonQuery();
        }

        private void MessagesWriter(object cancellationToken)
        {
            NpgsqlConnection Connention = new NpgsqlConnection(ConnectionString);
            Connention.Open();
            NpgsqlCommand command = Connention.CreateCommand();
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
                                for (int i = 0; i < 10000 && !messages.IsEmpty; i++)
                                {
                                    if (messages.TryDequeue(out Message message))
                                    {
                                        WriteSingleMessage(command, message);
                                        count++;
                                    }
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw ex;
                            }
                        }
                    }
                    Thread.Sleep(300);
                }
                catch (Exception ex)
                {
                    //logger.Error(ex, "Error while DB writing to DB");
                }



            }
        }

        private void UsersWriter(object cancellationToken)
        {
            NpgsqlConnection Connention = new NpgsqlConnection(ConnectionString);
            Connention.Open();
            NpgsqlCommand command = Connention.CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "add_user"; ;
            command.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("sender_username", NpgsqlTypes.NpgsqlDbType.Text));
            command.Parameters.Add(new NpgsqlParameter("sender_first_name", NpgsqlTypes.NpgsqlDbType.Bigint));
            command.Parameters.Add(new NpgsqlParameter("sender_last_name", NpgsqlTypes.NpgsqlDbType.Bigint));
            CancellationToken Token = (CancellationToken)cancellationToken;
            while (!Token.IsCancellationRequested)
            {
                try
                {
                    while (!users.IsEmpty)
                    {
                        using (NpgsqlTransaction transaction = Connention.BeginTransaction())
                        {
                            try
                            {
                                int count = 0;
                                for (int i = 0; i < 10000 && !users.IsEmpty; i++)
                                {
                                    if (users.TryDequeue(out Entity user))
                                    {
                                        WriteSingleUser(command, user);
                                        count++;
                                    }
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw ex;
                            }
                        }
                    }
                    Thread.Sleep(300);
                }
                catch (Exception ex)
                {
                    //logger.Error(ex, "Error while DB writing to DB");
                }



            }
        }

        public void PutUser(Entity entity)
        {
            if (entity.Type == EntityType.User)
            {
                users.Enqueue(entity);
            }
        }
        public void PutMessage(Message message)
        {
            messages.Enqueue(message);
        }
    }
}
