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
    public class ChatWriterCore : IWriterCore<Chat>
    {
        public string ConnectionString => Options.ConnectionString;

        public int TrasactionSize => Options.MessageWriterTrasactiobSize;

        public DbCommand CommandCreator(DbConnection connection)
        {
            DbCommand AddChatCommand = connection.CreateCommand();
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

        public void WriteSingleObject(DbCommand command, Chat chat)
        {
            command.Parameters["_chat_id"].Value = chat.Entity.Id;
            command.Parameters["_username"].Value = chat.Entity.Link.Equals(string.Empty) ? DBNull.Value : chat.Entity.Link;
            command.Parameters["_title"].Value = chat.Entity.FirstName;
            command.Parameters["pair_chat_id"].Value = chat.Entity.PairId != 0 ? chat.Entity.PairId : DBNull.Value;
            command.Parameters["_is_group"].Value = chat.Entity.Type == EntityType.Group;
            command.Parameters["_is_channel"].Value = chat.Entity.Type == EntityType.Channel;
            command.ExecuteNonQuery();
        }
    }
}
