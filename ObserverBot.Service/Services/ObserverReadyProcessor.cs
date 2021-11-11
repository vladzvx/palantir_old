using Bot.Core.Interfaces;
using Bot.Core.Models;
using Bot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ObserverBot.Service.Services
{
    public class ObserverReadyProcessor : IReadyProcessor<Bot.Core.Models.ObserverBot>
    {
        private readonly IMessagesSender messagesSender;
        public ObserverReadyProcessor(IMessagesSender messagesSender)
        {
            this.messagesSender = messagesSender;
        }

        public void SetConfig(IConfig config)
        {

        }

        public ISendedItem Stop(Update update)
        {
            return null;
        }

        public async Task ProcessUpdate(Update update, Bot.Core.Services.Bot.FSM<Bot.Core.Models.ObserverBot> fsm)
        {
            messagesSender.AddItem(new TextMessage(null, update.Message.Chat.Id, Environment.GetEnvironmentVariable("def_reply")?? "Оповещаю о появляющихся в телеграме упоминаниях. \nЕсли ничего не приходит - просто подождите какое-то время (возможно - несколько часов), и тему обязательно упомянут, а я сообщу вам об этом. \n\nНе выключайте оповещения!", null, new ReplyKeyboardRemove()));
        }
    }
}
