using Common.Services.DataBase;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.Core.Services
{
    public class BotMessageHandler : IUpdateHandler
    {
        private static string ConnectionString;
        private static long[] Selective;
        static BotMessageHandler()
        {
            ConnectionString = string.Format("User ID={0};Password={1};Host={2};Port={3};Database={4};Pooling=true;",
                Environment.GetEnvironmentVariable("DBUser"),
                Environment.GetEnvironmentVariable("DBPwd"),
                Environment.GetEnvironmentVariable("DBHost"),
                Environment.GetEnvironmentVariable("DBPort"),
                Environment.GetEnvironmentVariable("DBName")
                );
            try
            {
                Selective = Newtonsoft.Json.JsonConvert.DeserializeObject<long[]>(Environment.GetEnvironmentVariable("Selective"));
            }
            catch { }
        }
        public UpdateType[] AllowedUpdates => new UpdateType[] { UpdateType.Message};
        public BotMessageHandler()
        {

        }
        public async Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {

        }

        public async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string replyMessage = string.Empty;
            try
            {
                if (update.Type == UpdateType.Message)
                {
                    if (TryParseRequest(update.Message.Text, out string textForSearch))
                    {
                        if (Selective != null && Selective.Length > 0)
                        {
                            List<string> results = await SelectiveSearch(ConnectionString, textForSearch, 50);
                            if (results.Count > 0)
                            {
                                foreach (string res in results)
                                {
                                    if (replyMessage.Length + res.Length + 4 < 4000)
                                    {
                                        replyMessage += res + "\n";
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, replyMessage);
                                        await Task.Delay(1000);
                                        replyMessage = string.Empty;
                                    }
                                }
                                await botClient.SendTextMessageAsync(update.Message.Chat.Id, replyMessage);
                                await Task.Delay(1000);
                            }
                            else
                            {
                                replyMessage = "Нет подходящих результатов";
                                await botClient.SendTextMessageAsync(update.Message.Chat.Id, replyMessage);
                                await Task.Delay(1000);
                            }
                        }
                        else
                        {
                            List<string> results = await SimpleSearch(ConnectionString, textForSearch, 250);
                            if (results.Count > 0)
                            {
                                foreach (string res in results)
                                {
                                    if (replyMessage.Length + res.Length + 4 < 4000)
                                    {
                                        replyMessage += res + "\n";
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, replyMessage);
                                        await Task.Delay(1000);
                                        replyMessage = string.Empty;
                                    }
                                }
                                await botClient.SendTextMessageAsync(update.Message.Chat.Id, replyMessage);
                                await Task.Delay(1000);
                            }
                            else
                            {
                                replyMessage = "Нет подходящих результатов";
                                await botClient.SendTextMessageAsync(update.Message.Chat.Id, replyMessage);
                                await Task.Delay(1000);
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, ex.Message);
                await Task.Delay(1000);
            }

        }

        private static bool TryParseRequest(string txt, out string request)
        {
            Regex parseRegex = new Regex(@"^/search (.+)$");
            Match match = parseRegex.Match(txt);

            if (match.Success)
            {
                request = match.Groups[1].Value;
                return true;
            }
            else
            {
                request = string.Empty;
                return false;
            }
        }

        private static async Task<List<string>> SimpleSearch(string connectionString, string text, int limit)
        {
            try
            {
                using NpgsqlConnection Connection = new NpgsqlConnection(connectionString);
                await Connection.OpenAsync();
                NpgsqlCommand SimpleSearchCommand = Connection.CreateCommand();
                SimpleSearchCommand.CommandType = System.Data.CommandType.StoredProcedure;
                SimpleSearchCommand.Parameters.Add(new NpgsqlParameter("request", NpgsqlTypes.NpgsqlDbType.Text));
                SimpleSearchCommand.Parameters.Add(new NpgsqlParameter("lim", NpgsqlTypes.NpgsqlDbType.Integer));
                SimpleSearchCommand.CommandText = "simple_search";

                SimpleSearchCommand.Parameters["request"].Value = text;
                SimpleSearchCommand.Parameters["lim"].Value = limit;
                using NpgsqlDataReader reader = await SimpleSearchCommand.ExecuteReaderAsync();
                List<string> result = new List<string>();
                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(0))
                        result.Add(reader.GetString(0));
                }
                return result;
            }
            catch (Exception ex)
            {
                return new List<string>() { ex.Message };
            }
        }

        private static async Task<List<string>> SelectiveSearch(string connectionString, string text, int limit)
        {
            try
            {
                using NpgsqlConnection Connection = new NpgsqlConnection(connectionString);
                await Connection.OpenAsync();
                NpgsqlCommand SimpleSearchCommand = Connection.CreateCommand();
                SimpleSearchCommand.CommandType = System.Data.CommandType.StoredProcedure;
                SimpleSearchCommand.Parameters.Add(new NpgsqlParameter("request", NpgsqlTypes.NpgsqlDbType.Text));
                SimpleSearchCommand.Parameters.Add(new NpgsqlParameter("lim", NpgsqlTypes.NpgsqlDbType.Integer));
                SimpleSearchCommand.Parameters.Add(new NpgsqlParameter("chats", NpgsqlTypes.NpgsqlDbType.Array|NpgsqlTypes.NpgsqlDbType.Bigint));
                SimpleSearchCommand.CommandText = "simple_search_selective";

                SimpleSearchCommand.Parameters["request"].Value = text;
                SimpleSearchCommand.Parameters["lim"].Value = limit;
                SimpleSearchCommand.Parameters["chats"].Value = Selective;
                using NpgsqlDataReader reader = await SimpleSearchCommand.ExecuteReaderAsync();
                List<string> result = new List<string>();
                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(0))
                        result.Add(reader.GetString(0));
                }
                return result;
            }
            catch (Exception ex)
            {
                return new List<string>() { ex.Message };
            }
        }
    }
}
