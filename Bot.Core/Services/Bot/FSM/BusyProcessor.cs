using Bot.Core.Enums;
using Bot.Core.Interfaces;
using Bot.Core.Interfaces.BotFSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static Bot.Core.Services.Bot;

namespace Bot.Core.Services
{
    public class BusyProcessor : IBusyProcessor
    {
        private readonly IMessagesSender messagesSender;
        public BusyProcessor(IMessagesSender messagesSender)
        {
            this.messagesSender = messagesSender;
        }
        public async Task ProcessUpdate<TBot>(Update update, Bot.FSM<TBot> fsm) where TBot : IConfig, new()
        {
            if (Constants.Cancells.Contains(update.Message.Text.ToLower()))
            {
                ISendedItem reply = fsm.readyProcessor.Stop(update);
                messagesSender.AddItem(reply);
                fsm.config.BotState = PrivateChatState.Ready;
            }
            else
            {
                messagesSender.AddItem(Bot.Common.CreateImBusy(update));
            }
        }
    }
}
