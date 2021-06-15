using Common.Services.DataBase;
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
                        List<string> results = await SearchProvider.SimpleSearch(textForSearch, 250);
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
    }
}
