using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public partial class Bot
    {
        public partial class FSM
        {
            public static class Factory
            {
                public static DBWorker dBWorker;
                public static readonly ConcurrentDictionary<long, Services.Bot.FSM> state = new ConcurrentDictionary<long, Services.Bot.FSM>();
                public static async Task<Services.Bot.FSM> Get(ITelegramBotClient botClient, Update update, CancellationToken token)
                {
                    if (state.TryGetValue(update.Message.From.Id, out Services.Bot.FSM fsm))
                    {
                        return fsm;
                    }
                    else
                    {
                        FSM fSM = new FSM(botClient, update.Message.Chat.Id, await dBWorker.LogUser(update, token));
                        state.TryAdd(update.Message.From.Id, fSM);
                        return fSM;
                    }
                }

                public static async Task Load(ITelegramBotClient botClient)
                {
                    var temp = await dBWorker.GetAllUsers(CancellationToken.None);
                    foreach (var res in temp)
                    {
                        FSM fSM = new FSM(botClient, res.Id, res);
                        state.TryAdd(res.Id, fSM);
                    }
                }
            }
        }
    }
}
