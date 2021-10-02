using Bot.Core.Interfaces;
using Bot.Core.Interfaces.BotFSM;
using Bot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public class RightChecker : IRightChecker
    {
        private readonly IBotSettings botSettings;
        private readonly IMessagesSender messagesSender;
        public RightChecker(IBotSettings botSettings, IMessagesSender messagesSender)
        {
            this.botSettings = botSettings;
            this.messagesSender = messagesSender;
        }
        public async Task<bool> Check<TBot>(Update update, Bot.FSM<TBot> fsm) where TBot : IConfig, new()
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
            {
                if (fsm.config.Status > botSettings.BoundUserStatus)
                {
                    messagesSender.AddItem(new TextMessage(null, update.Message.Chat.Id, "Пожалуйста, обратитесь к администратору для выдачи разрешения на продолжение работы.", null));
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }
}
