using Bot.Core.Interfaces;
using Bot.Core.Models;
using Bot.Core.Services;
using ObserverBot.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ObserverBot.Service.Services
{
    public class ConfigurationProcessor : IConfigurationProcessor
    {
        private readonly IMessagesSender messagesSender;
        public ConfigurationProcessor(IMessagesSender messagesSender)
        {
            this.messagesSender = messagesSender;
        }
        public async Task<IConfig> ProcessUpdate(Update update, CancellationToken token)
        {
            messagesSender.AddItem(new TextMessage(null, update.Message.Chat.Id, "Оповещаю о появляющихся в телеграме упоминаниях космической тематики. \nЕсли ничего не приходит - просто подождите какое-то время (возможно - несколько часов), космос обязательно упомянут, а я сообщу вам об этом. \n\nНе выключайте оповещения!", null,new ReplyKeyboardRemove()));
            return new SearchBotConfig() {Finished=false };
        }
    }
}
