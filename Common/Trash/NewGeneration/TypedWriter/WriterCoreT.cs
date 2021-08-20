namespace Common.Services
{
    //public class WriterCore<T> : IWriterCore<T>
    //{
    //    public Task ExecuteAdditionaAcion(DbCommand command, object data, CancellationToken token)
    //    {
    //        throw new NotImplementedException();
    //    }
    //    public Task ExecuteAdditionaAcion(object data)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public DbCommand CreateMainCommand(DbConnection connection)
    //    {
    //        DbCommand Command = null;
    //        if (typeof(T) == typeof(Message))
    //        {

    //            Command.CommandType = System.Data.CommandType.StoredProcedure;
    //            Command.CommandText = "add_message";
    //            Command.Parameters.Add(new NpgsqlParameter("_message_timestamp", NpgsqlTypes.NpgsqlDbType.Timestamp));
    //            Command.Parameters.Add(new NpgsqlParameter("_message_id", NpgsqlTypes.NpgsqlDbType.Bigint));
    //            Command.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
    //            Command.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
    //            Command.Parameters.Add(new NpgsqlParameter("_reply_to", NpgsqlTypes.NpgsqlDbType.Bigint));
    //            Command.Parameters.Add(new NpgsqlParameter("_thread_start", NpgsqlTypes.NpgsqlDbType.Bigint));
    //            Command.Parameters.Add(new NpgsqlParameter("_media_group_id", NpgsqlTypes.NpgsqlDbType.Bigint));
    //            Command.Parameters.Add(new NpgsqlParameter("_forward_from_id", NpgsqlTypes.NpgsqlDbType.Bigint));
    //            Command.Parameters.Add(new NpgsqlParameter("_forward_from_message_id", NpgsqlTypes.NpgsqlDbType.Bigint));
    //            Command.Parameters.Add(new NpgsqlParameter("_text", NpgsqlTypes.NpgsqlDbType.Text));
    //            Command.Parameters.Add(new NpgsqlParameter("_media", NpgsqlTypes.NpgsqlDbType.Jsonb));
    //            Command.Parameters.Add(new NpgsqlParameter("_formatting", NpgsqlTypes.NpgsqlDbType.Jsonb));
    //        }
    //        else if (typeof(T) == typeof(User))
    //        {
    //            Command = connection.CreateCommand();
    //            Command.CommandType = System.Data.CommandType.StoredProcedure;
    //            Command.CommandText = "add_user"; ;
    //            Command.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
    //            Command.Parameters.Add(new NpgsqlParameter("sender_username", NpgsqlTypes.NpgsqlDbType.Text));
    //            Command.Parameters.Add(new NpgsqlParameter("sender_first_name", NpgsqlTypes.NpgsqlDbType.Text));
    //            Command.Parameters.Add(new NpgsqlParameter("sender_last_name", NpgsqlTypes.NpgsqlDbType.Text));
    //        }
    //        else if (typeof(T) == typeof(Chat))
    //        {
    //            Command = connection.CreateCommand();
    //            Command.CommandType = System.Data.CommandType.StoredProcedure;
    //            Command.CommandText = "add_chat"; ;
    //            Command.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
    //            Command.Parameters.Add(new NpgsqlParameter("_title", NpgsqlTypes.NpgsqlDbType.Text));
    //            Command.Parameters.Add(new NpgsqlParameter("_username", NpgsqlTypes.NpgsqlDbType.Text));
    //            Command.Parameters.Add(new NpgsqlParameter("pair_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
    //            Command.Parameters.Add(new NpgsqlParameter("_is_group", NpgsqlTypes.NpgsqlDbType.Boolean));
    //            Command.Parameters.Add(new NpgsqlParameter("_is_channel", NpgsqlTypes.NpgsqlDbType.Boolean));
    //        }
    //        else if (typeof(T) == typeof(Ban))
    //        {
    //            Command = connection.CreateCommand();
    //            Command.CommandType = System.Data.CommandType.Text;
    //            Command.CommandText = "update chats set banned = true where id = @_id"; ;
    //            Command.Parameters.Add(new NpgsqlParameter("_id", NpgsqlTypes.NpgsqlDbType.Bigint));
    //        }
    //        else if (typeof(T) == typeof(Deleting))
    //        {
    //            Command = connection.CreateCommand();
    //            Command.CommandType = System.Data.CommandType.Text;
    //            Command.CommandText = "delete from messages where message_db_id=@_id;";
    //            Command.Parameters.Add(new NpgsqlParameter("_id", NpgsqlTypes.NpgsqlDbType.Bigint));
    //        }
    //        else throw new InvalidCastException();
    //        return Command;
    //    }

    //    public async Task ExecuteWriting(DbCommand Command, T data, CancellationToken token)
    //    {
    //        if (Command == null) throw new MissingFieldException("Command was not created! Please use CreateCommand method");
    //        if (data is Message message && Command != null)
    //        {
    //            DbCommand command = Command;
    //            //command.Transaction = transaction;
    //            command.Parameters["_message_timestamp"].Value = message.Timestamp.ToDateTime();
    //            command.Parameters["_message_id"].Value = message.Id;
    //            command.Parameters["_chat_id"].Value = message.ChatId;
    //            command.Parameters["_user_id"].Value = message.FromId != 0 ? message.FromId : DBNull.Value;
    //            command.Parameters["_reply_to"].Value = message.ReplyTo != 0 ? message.ReplyTo : DBNull.Value;
    //            command.Parameters["_thread_start"].Value = message.ThreadStart != 0 ? message.ThreadStart : DBNull.Value;
    //            command.Parameters["_media_group_id"].Value = message.MediagroupId != 0 ? message.MediagroupId : DBNull.Value;
    //            command.Parameters["_forward_from_id"].Value = message.ForwardFromId != 0 ? message.ForwardFromId : DBNull.Value;
    //            command.Parameters["_forward_from_message_id"].Value = message.ForwardFromMessageId != 0 ? message.ForwardFromMessageId : DBNull.Value;
    //            command.Parameters["_text"].Value = !string.IsNullOrEmpty(message.Text) ? message.Text : DBNull.Value;
    //            command.Parameters["_media"].Value = string.IsNullOrEmpty(message.Media) ? DBNull.Value : message.Media;
    //            command.Parameters["_formatting"].Value = message.Formating.Count == 0 || Formating.IsEmpty(message.Formating) ?
    //                DBNull.Value :
    //                "{\"formats\":" + Newtonsoft.Json.JsonConvert.SerializeObject(message.Formating) + "}";
    //            await command.ExecuteNonQueryAsync(token);
    //            return;
    //        }

    //        if (data is Chat chat && Command != null)
    //        {
    //            DbCommand command = Command;
    //            //command.Transaction = transaction;
    //            command.Parameters["_chat_id"].Value = chat.Entity.Id;
    //            command.Parameters["_username"].Value = chat.Entity.Link.Equals(string.Empty) ? DBNull.Value : chat.Entity.Link;
    //            command.Parameters["_title"].Value = chat.Entity.FirstName;
    //            command.Parameters["pair_chat_id"].Value = chat.Entity.PairId != 0 ? chat.Entity.PairId : DBNull.Value;
    //            command.Parameters["_is_group"].Value = chat.Entity.Type == EntityType.Group;
    //            command.Parameters["_is_channel"].Value = chat.Entity.Type == EntityType.Channel;
    //            await command.ExecuteNonQueryAsync(token);
    //            return;
    //        }

    //        if (data is User user && Command != null)
    //        {
    //            DbCommand command = Command;
    //            //command.Transaction = transaction;
    //            command.Parameters["_user_id"].Value = user.Entity.Id;
    //            command.Parameters["sender_username"].Value = user.Entity.Link;
    //            command.Parameters["sender_first_name"].Value = user.Entity.FirstName;
    //            command.Parameters["sender_last_name"].Value = user.Entity.LastName;
    //            await command.ExecuteNonQueryAsync(token);
    //            return;

    //        }

    //        if (data is Ban ban && Command != null)
    //        {
    //            DbCommand command = Command;
    //            //command.Transaction = transaction;
    //            command.Parameters["_id"].Value = ban.Entity.Id;
    //            await command.ExecuteNonQueryAsync(token);
    //            return;
    //        }

    //        if (data is Deleting del && Command!=null)
    //        {
    //            DbCommand command = Command;
    //           // command.Transaction = transaction;
    //            command.Parameters["_id"].Value = del.message_db_id;
    //            await command.ExecuteNonQueryAsync(token);
    //            return;
    //        }
    //    }

    //    public Task ExecuteAdditionaAcion(object data, CancellationToken token)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Task ExecuteAdditionaAcion(DbConnection dbConnection, object data, CancellationToken token)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

}
