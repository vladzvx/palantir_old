using Bot.Core.Enums;
using Bot.Core.Interfaces;
using Bot.Core.Models;
using Common.Services;
using Common.Services.Interfaces;
using MongoDB.Bson;
using System;
using System.Linq;
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
        public static MongoWriter writer;
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
            if (update.Type == UpdateType.Message || update.Type == UpdateType.ChatMember)
            {
                await writer.PutData(update);
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
                else if (update.CallbackQuery.Data.Split('_').Length==3)
                {
                    var spl = update.CallbackQuery.Data.Split('_');
                    if (long.TryParse(spl[0], out long chatId)&& long.TryParse(spl[1], out long userId)&& Enum.TryParse<Command>(spl[2],out var command))
                    {
                        var b = new TBot()
                        {
                            Id = userId,
                            userType = UserType.User,
                        };

                        if ((await TextMessage.defaultClient.GetChatAdministratorsAsync(chatId)).ToList().FindIndex(item=>item.User.Id== update.CallbackQuery.From.Id)>=0)
                        {
                            switch (command)
                            {
                                case Command.Ban:
                                    {
                                        b.Status = UserStatus.banned;
                                        asyncTaskExecutor.Add(TextMessage.defaultClient.KickChatMemberAsync(chatId, userId, revokeMessages: true));

                                        break;
                                    }
                                case Command.Trust:
                                    {
                                        b.Status = UserStatus.common;
                                        break;
                                    }
                            }
                            asyncTaskExecutor.Add(Bot.FSM<TBot>.Factory.dBWorker.SaveChat(b, cancellationToken, TextMessage.defaultClient.BotId.Value));
                        }

                    }
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
