using Bot.Core.Enums;
using Bot.Core.Interfaces;
using Bot.Core.Models;
using Bot.Core.Services;
using Common.Interfaces;
using Common.Services;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GateKeeper.Service.Services
{
    public class GateKeeperReadyProcessor : IReadyProcessor<Bot.Core.Models.GateKeeperBot>
    {
        private readonly IDataStorage<GateKeeperBot> dataStorage;
        private readonly AsyncTaskExecutor asyncTaskExecutor;
        private readonly IUserChecker userChecker;
        private readonly IMessagesSender messagesSender;
        private readonly object locker = new object();
        
        public GateKeeperReadyProcessor(IDataStorage<GateKeeperBot> dataStorage, AsyncTaskExecutor asyncTaskExecutor, IUserChecker userChecker,IMessagesSender messagesSender)
        {
            this.dataStorage = dataStorage;
            this.asyncTaskExecutor = asyncTaskExecutor;
            this.userChecker = userChecker;
            this.messagesSender = messagesSender;
        }

        public void SetConfig(IConfig config)
        {

        }

        public ISendedItem Stop(Update update)
        {
            return null;
        }

        public async Task ProcessUpdate(Update update, Bot.Core.Services.Bot.FSM<Bot.Core.Models.GateKeeperBot> fsm)
        {
            asyncTaskExecutor.Add(ProcessUpdateAsyncWr(update, fsm));
        }


        public async Task<bool> TryChangeStatus(Update update, Bot.Core.Services.Bot.FSM<Bot.Core.Models.GateKeeperBot> fsm)
        {
            if ((update.Message.Chat.Type==Telegram.Bot.Types.Enums.ChatType.Group|| update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)&&(await TextMessage.defaultClient.GetChatAdministratorsAsync(update.Message.Chat.Id)).ToList().FindIndex(item => item.User.Id == update.Message.From.Id) >= 0)
            {
                if (update.Message.Text.ToLower().StartsWith("/overrun"))
                {
                    fsm.config.Mode = ChatState.Overrun;
                }
                else if (update.Message.Text.ToLower().StartsWith("/common"))
                {
                    fsm.config.Mode = ChatState.Common;
                }
                return false;
            }
            return true;
        }
        private async Task Check(long userId,string Username, string Name, long chatId, int messageId,Bot.Core.Services.Bot.FSM<Bot.Core.Models.GateKeeperBot> fsm)
        {
            var res = await userChecker.Check(userId);
            var user = new GateKeeperBot()
            {
                Id = userId,
                Name = Name,
                Username = Username,
                userType = Bot.Core.Enums.UserType.User,
                Status = Bot.Core.Enums.UserStatus.common
            };
            if (res.Status <= Common.Enums.UserStatus.SimpleBad)
            {
                if (fsm.config.Mode == ChatState.Overrun)
                {
                    await TextMessage.defaultClient.KickChatMemberAsync(chatId, userId, revokeMessages: true);
                    user.Status = Bot.Core.Enums.UserStatus.banned;
                    await dataStorage.SaveChat(user, CancellationToken.None, TextMessage.defaultClient.BotId.Value);
                }
                //else
                //{
                //    string commandBan = string.Format("{0}_{1}_{2}", chatId, userId, Command.Ban.ToString());
                //    string commandTrust = string.Format("{0}_{1}_{2}", chatId, userId, Command.Trust.ToString());
                //    InlineKeyboardMarkup keyb = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
                //    {
                //        new List<InlineKeyboardButton>()
                //        {
                //            new InlineKeyboardButton()
                //            {
                //                CallbackData=commandBan,
                //                Text= "Бан"
                //            }
                //        }
                //    });
                //    TextMessage textMessage = new TextMessage(null, chatId, Environment.GetEnvironmentVariable("PreBanMessage") ?? "Обнаружена антивакса! "+"Score: "+Math.Round(res.Score,3).ToString() +" Выберете действие:", null, null, messageId, null, keyb);
                //    messagesSender.AddItem(textMessage);
                //}

            }
            else if (res.Status == Common.Enums.UserStatus.Middle)
            {
                string commandBan = string.Format("{0}_{1}_{2}", chatId, userId, Command.Ban.ToString());
                string commandTrust = string.Format("{0}_{1}_{2}", chatId, userId, Command.Trust.ToString());
                string commandWait = string.Format("{0}_{1}_{2}", chatId, userId, Command.Wait.ToString());
                string commandSearch = res.FirstPageId != ObjectId.Empty ? res.FirstPageId.ToString() : string.Empty;
                var keybBase = new List<List<InlineKeyboardButton>>()
                    {
                        new List<InlineKeyboardButton>()
                        {
                            new InlineKeyboardButton()
                            {
                                CallbackData=commandBan,
                                Text= "Бан"
                            },
                            new InlineKeyboardButton()
                            {
                                CallbackData=commandTrust,
                                Text= "Оправдать"
                            },
                        }
                    };
                if (!string.IsNullOrEmpty(commandSearch))
                {
                    keybBase.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton() {Text="Личное дело",CallbackData= commandSearch } });
                }
                InlineKeyboardMarkup keyb = new InlineKeyboardMarkup(keybBase);
                TextMessage textMessage = new TextMessage(null, chatId, Environment.GetEnvironmentVariable("PreBanMessage") ?? "Обнаружен потенциальный антипрививочник! \n\nДля просмотра личного дела нужно иметь права администратора и стартовать личный диалог со мной." + "Соц рейтинг: " + Math.Round(res.Score, 3).ToString() + "\n\n Выберете действие:", null, null, messageId, null, keyb);
                messagesSender.AddItem(textMessage);
            }


        }
        public async Task ProcessUpdateAsyncWr(Update update, Bot.Core.Services.Bot.FSM<Bot.Core.Models.GateKeeperBot> fsm)
        {
            if (await TryChangeStatus(update, fsm))
            {
                Regex reg = new Regex(@"(\d+)");
                if (update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
                {
                    Match match = reg.Match(update.Message.Text.ToLower());
                    if (match.Success && long.TryParse(match.Groups[1].Value, out long userId))
                    {
                        var res = await userChecker.Check(userId);
                        TextMessage textMessage = new TextMessage(null, update.Message.Chat.Id, Newtonsoft.Json.JsonConvert.SerializeObject(res), null, null, update.Message.MessageId, null, null);
                        messagesSender.AddItem(textMessage);
                    }
                }
                else
                {
                    await ProcessUpdateAsync(update, fsm);
                }

            }
        }
        private async Task ProcessUser(User user, Bot.Core.Services.Bot.FSM<Bot.Core.Models.GateKeeperBot> fsm, Update update)
        {
            var temp = await dataStorage.GetChat(user.Id, CancellationToken.None, TextMessage.defaultClient.BotId.Value);
            if (temp != null && temp.Status == Bot.Core.Enums.UserStatus.banned)
            {
                if (fsm.config.Mode == ChatState.Overrun)
                {
                    await TextMessage.defaultClient.KickChatMemberAsync(update.Message.Chat.Id, user.Id, revokeMessages: true);
                }
                else
                {
                    long chatId = update.Message.Chat.Id;
                    long userId = user.Id;
                    string commandBan = string.Format("{0}_{1}_{2}", chatId, userId, Command.Ban.ToString());
                    InlineKeyboardMarkup keyb = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
                    {
                        new List<InlineKeyboardButton>()
                        {
                            new InlineKeyboardButton()
                            {
                                CallbackData=commandBan,
                                Text= "Ban"
                            }
                        }
                    });
                    TextMessage textMessage = new TextMessage(null, update.Message.Chat.Id, Environment.GetEnvironmentVariable("PreBanMessage") ?? "Обнаружен подтверждённый антипрививочник!", null, null, update.Message.MessageId, null, keyb);
                    messagesSender.AddItem(textMessage);
                }
            }
            else if (temp == null)
            {
                await Check(update.Message.From.Id, update.Message.From.Username, update.Message.From.FirstName, update.Message.Chat.Id, update.Message.MessageId, fsm);
            }
        }
        public async Task ProcessUpdateAsync(Update update, Bot.Core.Services.Bot.FSM<Bot.Core.Models.GateKeeperBot> fsm)
        {
            if (update.Message.NewChatMembers != null)
            {
                foreach (User user in update.Message.NewChatMembers)
                {
                    await ProcessUser(user, fsm, update);
                }
            }
            else if (update.ChatMember!=null && update.ChatMember.NewChatMember!=null)
            {
                var user = update.ChatMember.NewChatMember.User;
                await ProcessUser(user,fsm,update);
            }
            else
            {
                var ch = await TextMessage.defaultClient.GetChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id);
                if (ch.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Left)
                {
                    await ProcessUser(ch.User, fsm, update);
                }
            }
        }
    }
}
