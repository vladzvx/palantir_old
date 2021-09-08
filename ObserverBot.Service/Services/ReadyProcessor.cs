using Bot.Core.Interfaces;
using Bot.Core.Models;
using Bot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ObserverBot.Service.Services
{
    public class EmptyReadyProcessor : IReadyProcessor
    {
        public SearchBotConfig config =>SearchBotConfig.Default;
        private readonly IMessagesSender messagesSender;
        public EmptyReadyProcessor(IMessagesSender messagesSender)
        {
            this.messagesSender = messagesSender;
        }
        public async Task ProcessUpdate(Update update, Func<object, Task> func)
        {
            //messagesSender.AddItem(new TextMessage(null,update.Message.Chat.Id, "Оповещаю о появляющихся в телеграме упоминаниях космической тематики. \n\nЕсли ничего не приходит - просто подождите какое-то время (возможно - несколько часов), космос обязательно упомянут, а я сообщу вам об этом. \n\nНе выключайте оповещения!:)", null));
        }

        public void SetConfig(IConfig config)
        {

        }

        public ISendedItem Stop(Update update)
        {
            return null;
        }
    }
}
