using Bot.Core.Enums;
using Bot.Core.Models;
using Common.Services.DataBase;
using Npgsql;
using System;
using System.Data.Common;
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

        public async Task<Services.SearchBotConfig> LogUser(Update update, CancellationToken token, Services.SearchBotConfig settings = null)
        {
            using (var conn = await connectionsFactory.GetConnectionAsync(token))
            {
                DbCommand command = conn.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "log_user";
                command.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
                command.Parameters.Add(new NpgsqlParameter("sender_username", NpgsqlTypes.NpgsqlDbType.Text));
                command.Parameters.Add(new NpgsqlParameter("sender_first_name", NpgsqlTypes.NpgsqlDbType.Text));
                command.Parameters.Add(new NpgsqlParameter("_state", NpgsqlTypes.NpgsqlDbType.Integer));
                command.Parameters.Add(new NpgsqlParameter("_search_in_groups", NpgsqlTypes.NpgsqlDbType.Boolean));
                command.Parameters.Add(new NpgsqlParameter("_search_in_channels", NpgsqlTypes.NpgsqlDbType.Boolean));
                command.Parameters.Add(new NpgsqlParameter("_depth", NpgsqlTypes.NpgsqlDbType.Integer));

                command.Parameters["_user_id"].Value = update.Message.From.Id;
                command.Parameters["sender_username"].Value = update.Message.From.Username == null ? DBNull.Value : update.Message.From.Username;
                command.Parameters["sender_first_name"].Value = update.Message.From.FirstName == null ? DBNull.Value : update.Message.From.FirstName;
                command.Parameters["_state"].Value = settings == null ? DBNull.Value : (int)settings.BotState;
                command.Parameters["_search_in_groups"].Value = settings == null ? DBNull.Value : settings.SearchInGroups;
                command.Parameters["_search_in_channels"].Value = settings == null ? DBNull.Value : settings.SearchInChannels;
                command.Parameters["_depth"].Value = settings == null ? DBNull.Value : (int)settings.RequestDepth;
                try
                {
                    using (var reader = await command.ExecuteReaderAsync(token))
                    {
                        while (await reader.ReadAsync(token))
                        {
                            if (!await reader.IsDBNullAsync(0))
                            {
                                int status = reader.GetInt32(0);
                                int state = reader.GetInt32(1);
                                int depth = reader.GetInt32(4);
                                bool searchInGroups = reader.GetBoolean(2);
                                bool searchInChannels = reader.GetBoolean(3);
                                if (Enum.IsDefined(typeof(UserStatus), status) &&
                                    Enum.IsDefined(typeof(PrivateChatState), state) &&
                                    Enum.IsDefined(typeof(RequestDepth), depth))
                                {
                                    Services.SearchBotConfig result = new Services.SearchBotConfig();
                                    result.BotState = (PrivateChatState)state;
                                    result.Status = (UserStatus)status;
                                    result.RequestDepth = (RequestDepth)depth;
                                    result.SearchInChannels = searchInChannels;
                                    result.SearchInGroups = searchInGroups;
                                    return result;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }

                return new Services.SearchBotConfig();
            }
        }

        public async Task SavePage(Page page, CancellationToken token)
        {
            using (var conn = await connectionsFactory.GetConnectionAsync(token))
            {
                DbCommand command = conn.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "save_page";
                command.Parameters.Add(new NpgsqlParameter("_page", NpgsqlTypes.NpgsqlDbType.Integer));
                command.Parameters.Add(new NpgsqlParameter("_search_guid", NpgsqlTypes.NpgsqlDbType.Text));
                command.Parameters.Add(new NpgsqlParameter("_data", NpgsqlTypes.NpgsqlDbType.Jsonb));

                command.Parameters["_page"].Value = page.Number;
                command.Parameters["_search_guid"].Value = page.SearchGuid.ToString();
                command.Parameters["_data"].Value = Newtonsoft.Json.JsonConvert.SerializeObject(page);
                await command.ExecuteNonQueryAsync(token);
            }
        }

        public async Task<Page> GetPage(Guid guid, int number, CancellationToken token)
        {
            using (var conn = await connectionsFactory.GetConnectionAsync(token))
            {
                DbCommand command = conn.Connection.CreateCommand();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "get_page";
                command.Parameters.Add(new NpgsqlParameter("_page", NpgsqlTypes.NpgsqlDbType.Integer));
                command.Parameters.Add(new NpgsqlParameter("_search_guid", NpgsqlTypes.NpgsqlDbType.Text));

                command.Parameters["_page"].Value = number;
                command.Parameters["_search_guid"].Value = guid.ToString();

                using (var reader = await command.ExecuteReaderAsync(token))
                {
                    while (await reader.ReadAsync(token))
                    {
                        if (!await reader.IsDBNullAsync(0))
                        {
                            string result = reader.GetString(0);
                            return Newtonsoft.Json.JsonConvert.DeserializeObject<Page>(result);
                        }
                    }
                }
                return Page.Empty;
            }
        }
    }
}
