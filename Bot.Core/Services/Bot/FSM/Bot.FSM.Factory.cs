﻿using Bot.Core.Interfaces;
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
                    if (state.TryGetValue(update.Message.Chat.Id, out Services.Bot.FSM<TBot> fsm))
                    {
                        return fsm;
                    }
                    else
                    {
                        if (TextMessage.defaultClient != null && TextMessage.defaultClient.BotId.HasValue) 
                        {

                            var temp = await dBWorker.GetChat(update.Message.Chat.Id, token, TextMessage.defaultClient.BotId.Value);
                            if (temp == null)
                            {
                                temp = new TBot() { Id = update.Message.Chat.Id };
                            }

                            if (update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
                            {
                                temp.Username = update.Message.From.Username;
                                temp.Name = update.Message.From.FirstName;
                                temp.userType = Enums.UserType.User;
                            }
                            else if (update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group || update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup && update.Message.From.Id != 777000)
                            {
                                try
                                {
                                    temp.Username = update.Message.Chat.Username;
                                    temp.Name = update.Message.Chat.FirstName;
                                    temp.userType = Enums.UserType.Group;
                                    //await dBWorker.SaveChat(temp, token, TextMessage.defaultClient.BotId.Value);
                                    //var temp2 = await dBWorker.GetChat(update.Message.From.Id, token, TextMessage.defaultClient.BotId.Value);
                                    //if (temp2 == null)
                                    //{
                                    //    temp2 = new TBot() { Id = update.Message.From.Id };
                                    //    temp2.Username = update.Message.From.Username;
                                    //    temp2.Name = update.Message.From.FirstName;
                                    //    temp2.userType = Enums.UserType.User;
                                    //    temp2.Status = Enums.UserStatus.common;
                                    //}
                                    //else
                                    //{
                                    //    temp2.Username = update.Message.From.Username;
                                    //    temp2.Name = update.Message.From.FirstName;
                                    //    temp2.userType = Enums.UserType.User;
                                    //}
                                    //await dBWorker.SaveChat(temp2, token, TextMessage.defaultClient.BotId.Value);
                                }
                                catch (Exception ex)
                                {

                                }
                            }

                            await dBWorker.SaveChat(temp, token, TextMessage.defaultClient.BotId.Value);
                            FSM<TBot> fSM = new FSM<TBot>(update.Message.Chat.Id, temp);
                            state.TryAdd(update.Message.Chat.Id, fSM);
                            return fSM;
                        }
                        return null;
                    }
                }

                
                public static async Task Load(CancellationToken token)
                {
                    foreach (var bot in await dBWorker.GetAllIds(token, TextMessage.defaultClient.BotId.Value))
                    {
                        if (!state.ContainsKey(bot.Id))
                        {
                            state.TryAdd(bot.Id, new FSM<TBot>(bot.Id, bot));
                        }

                    }
                }
            }
        }
    }
}
