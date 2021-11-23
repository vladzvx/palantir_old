using Bot.Core.Enums;
using Bot.Core.Interfaces;
using Bot.Core.Models;
using Common.Services;
using Common.Services.Interfaces;
using MongoDB.Bson;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Core.Services
{
    public class MessageHandler<TBot> : IUpdateHandler where TBot:IConfig,new()
    {
        public static ICommonWriter<Message> writer;
        public static AsyncTaskExecutor asyncTaskExecutor;
        public static SearchState searchState;
        public static IBotSettings botSettings;
        public UpdateType[] AllowedUpdates => new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery };

        public Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
            {
                Bot.FSM<TBot> processor = await Bot.FSM<TBot>.Factory.Get(update, cancellationToken);
                await processor.Process(update, cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                if (ObjectId.TryParse(update.CallbackQuery.Data, out ObjectId guid))
                {
                    Task sending = searchState.TryEdit(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, guid, cancellationToken);
                    asyncTaskExecutor.Add(sending);
                }
                else
                {
                    Task sending = searchState.TryEdit(update, cancellationToken);
                    asyncTaskExecutor.Add(sending);
                }
            }
        }
    }
}
