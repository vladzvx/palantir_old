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
using Common.Services.DataBase.Interfaces;

namespace Common.Services
{
    public class ChatInfoReader : ICommonReader<ChatInfo>
    {
        private readonly ConnectionsFactory connectionsFactory;
        public ChatInfoReader(ConnectionsFactory connectionsFactory)
        {
            this.connectionsFactory = connectionsFactory;
        }
        public async Task<ChatInfo> ReadAsync(object parameters, CancellationToken token)
        {
            using (ConnectionWrapper connectionWrapper = await connectionsFactory.GetConnectionAsync(token))
            {
                if (parameters is ChatInfoRequest request)
                {
                    DbCommand additionalCommand = connectionWrapper.Connection.CreateCommand();
                    additionalCommand.CommandType = System.Data.CommandType.Text;
                    additionalCommand.CommandText = "select last_message_id from chats where id=@_chat_id";
                    additionalCommand.Parameters.Add(new NpgsqlParameter("_chat_id", NpgsqlTypes.NpgsqlDbType.Bigint));
                    additionalCommand.Parameters["_chat_id"].Value = request.Id;
                    DbDataReader reader = await additionalCommand.ExecuteReaderAsync(token);
                    while (await reader.ReadAsync(token))
                    {
                        long offset = 1;
                        if (!await reader.IsDBNullAsync(0))
                        {
                            offset = reader.GetInt64(0);
                        }
                        return new ChatInfo() {Id=request.Id,Offset=offset };
                    }
                }
                throw new Exception("Reading failed");
            }
        }
    }
}


