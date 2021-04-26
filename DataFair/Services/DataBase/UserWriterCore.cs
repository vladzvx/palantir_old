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
    public class UserWriterCore : IWriterCore<User>
    {
        public string ConnectionString => Options.ConnectionString;

        public int TrasactionSize => Options.MessageWriterTrasactiobSize;

        public DbCommand CommandCreator(DbConnection connection)
        {
            DbCommand AddUserCommand = connection.CreateCommand();
            AddUserCommand.CommandType = System.Data.CommandType.StoredProcedure;
            AddUserCommand.CommandText = "add_user"; ;
            AddUserCommand.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            AddUserCommand.Parameters.Add(new NpgsqlParameter("sender_username", NpgsqlTypes.NpgsqlDbType.Text));
            AddUserCommand.Parameters.Add(new NpgsqlParameter("sender_first_name", NpgsqlTypes.NpgsqlDbType.Text));
            AddUserCommand.Parameters.Add(new NpgsqlParameter("sender_last_name", NpgsqlTypes.NpgsqlDbType.Text));
            return AddUserCommand;
        }

        public void WriteSingleObject(DbCommand command, User user)
        {
            command.Parameters["_user_id"].Value = user.Entity.Id;
            command.Parameters["sender_username"].Value = user.Entity.Link;
            command.Parameters["sender_first_name"].Value = user.Entity.FirstName;
            command.Parameters["sender_last_name"].Value = user.Entity.LastName;
            command.ExecuteNonQuery();
        }
    }
}
