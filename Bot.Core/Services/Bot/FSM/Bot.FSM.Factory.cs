using Bot.Core.Interfaces;
using Bot.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Services
{
    public partial class Bot
    {

        public partial class FSM<TBot> where TBot:IConfig, new()
        {
            public static class Factory
            {
                public static IDataStorage<TBot> dBWorker;
                public static readonly ConcurrentDictionary<long, Services.Bot.FSM<TBot>> state = new ConcurrentDictionary<long, Services.Bot.FSM<TBot>>();
                public static readonly ConcurrentDictionary<long, DateTime> stateTimestamps = new ConcurrentDictionary<long, DateTime>();
                public static async Task<Services.Bot.FSM<TBot>> Get(Update update, CancellationToken token)
                {
                    if (state.TryGetValue(update.Message.From.Id, out Services.Bot.FSM<TBot> fsm))
                    {
                        return fsm;
                    }
                    else
                    {
                        var temp = await dBWorker.GetChat(update.Message.Chat.Id, token);
                        if (temp == null)
                        {
                            temp = new TBot() { Id = update.Message.Chat.Id };
                            await dBWorker.SaveChat(temp, token);
                        }
                        FSM<TBot> fSM = new FSM<TBot>(update.Message.Chat.Id, temp);
                        state.TryAdd(update.Message.From.Id, fSM);
                        return fSM;
                    }
                }
            }
        }
    }
}
