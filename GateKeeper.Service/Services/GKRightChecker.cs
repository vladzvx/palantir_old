using Bot.Core.Interfaces;
using Bot.Core.Interfaces.BotFSM;
using Bot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GateKeeper.Service
{
    public class GKRightChecker : IRightChecker
    {
        private readonly IBotSettings botSettings;
        private readonly IMessagesSender messagesSender;
        public GKRightChecker(IBotSettings botSettings, IMessagesSender messagesSender)
        {
            this.botSettings = botSettings;
            this.messagesSender = messagesSender;
        }
        public async Task<bool> Check<TBot>(Update update, Bot.Core.Services.Bot.FSM<TBot> fsm) where TBot : IConfig, new()
        {
            return (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && (update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group|| update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup || update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)  && update.Message.From.Id!=777000 && fsm.config.Status <= botSettings.BoundUserStatus);
        }
    }
}
