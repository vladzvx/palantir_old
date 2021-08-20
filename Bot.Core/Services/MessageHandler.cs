using Common.Services;
using Common.Services.Interfaces;
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
    public class MessageHandler : IUpdateHandler
    {
        public static ICommonWriter<Message> writer;
        public static AsyncTaskExecutor asyncTaskExecutor;
        public static SearchState searchState;

        public UpdateType[] AllowedUpdates => new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery };

        public Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
            {
                Bot.FSM processor = await Bot.FSM.Factory.Get(botClient, update, cancellationToken);
                await processor.ProcessUpdate(update, cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                string[] splittedId = update.CallbackQuery.Data.Split('_');
                if (splittedId.Length == 2 &&
                    int.TryParse(splittedId[1], out int id) &&
                    Guid.TryParse(splittedId[0], out Guid guid))
                {
                    if (id >= 0)
                    {
                        Task sending = searchState.TrySendPage(update.CallbackQuery.From.Id, guid, id, cancellationToken);
                        asyncTaskExecutor.Add(sending);
                    }
                }
            }

        }
    }
}
