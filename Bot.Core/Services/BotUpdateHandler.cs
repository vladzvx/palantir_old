using Common;
using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Common.Services.gRPC;
using Grpc.Core;
using Grpc.Net.Client;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Core.Services
{
    public class BotMessageHandler2 : IUpdateHandler
    {
        
        private ConcurrentDictionary<long, Bot.FinitStateMachine> sessions = new ConcurrentDictionary<long, Bot.FinitStateMachine>();
        public UpdateType[] AllowedUpdates => new UpdateType[] { UpdateType.Message};

        public async Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {

        }

        public async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (sessions.TryGetValue(update.Message.From.Id, out Bot.FinitStateMachine state))
            {
                await state.ProcessUpdate(update);
            }
            else
            {
                state = new Bot.FinitStateMachine(botClient, update.Message.Chat.Id);
                sessions.TryAdd(update.Message.From.Id, state);
                await state.ProcessUpdate(update);
            }
        }
    }
}
