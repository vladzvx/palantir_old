using Bot.Core.Enums;
using Common.Services.DataBase;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core
{
    public class DBWorker
    {
        private readonly ConnectionsFactory connectionsFactory;
        public DBWorker(ConnectionsFactory connectionsFactory)
        {
            this.connectionsFactory = connectionsFactory;
        }

        public async Task<UserStatus> LogUser(Update update, CancellationToken token)
        {
            using (var conn = await connectionsFactory.GetConnectionAsync(token))
            {
                DbCommand command = conn.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "add_user";
                command.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
                command.Parameters.Add(new NpgsqlParameter("sender_username", NpgsqlTypes.NpgsqlDbType.Text));
                command.Parameters.Add(new NpgsqlParameter("sender_first_name", NpgsqlTypes.NpgsqlDbType.Text));

                command.Parameters["_user_id"].Value = update.Message.From.Id;
                command.Parameters["sender_username"].Value = update.Message.From.Username == null ? DBNull.Value: update.Message.From.Username;
                command.Parameters["sender_first_name"].Value = update.Message.From.FirstName == null ? DBNull.Value : update.Message.From.FirstName;

                using (var reader = await command.ExecuteReaderAsync(token))
                {
                    while (await reader.ReadAsync(token))
                    {
                        if (!await reader.IsDBNullAsync(0))
                        {
                            int result = reader.GetInt32(0);
                            if (Enum.IsDefined(typeof(UserStatus), result))
                            {
                                return (UserStatus)result;
                            }
                        }
                    }
                }
                return UserStatus.common;
            }
        }
    }
}
