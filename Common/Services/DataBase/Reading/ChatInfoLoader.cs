using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase.Reading
{
    public class ChatInfoLoader
    {
        private readonly ConnectionsFactory connectionsFactory;
        public ChatInfoLoader(ConnectionsFactory connectionsFactory)
        {
            this.connectionsFactory = connectionsFactory;
        }

        public async Task<List<Entity>> Read(CancellationToken cancellationToken)
        {
            List<Entity> result = new List<Entity>();
            using (var cnn = await connectionsFactory.GetConnectionAsync(cancellationToken))
            {

                var command = cnn.Connection.CreateCommand();
                command.CommandText = "select id, username, is_channel from chats;";
                command.CommandType = System.Data.CommandType.Text;
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        if (!reader.IsDBNull(1) && !reader.IsDBNull(2))
                        {
                            bool is_channel = reader.GetBoolean(2);
                            result.Add(new Entity() {Id=reader.GetInt64(0),Link=reader.GetString(1),
                                 Type = is_channel ? EntityType.Channel:EntityType.Group });
                        }
                    }
                }
            }
            return result;
        }
    }
}
