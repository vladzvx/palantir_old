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
    public class SubFSMFactory : IFSMFactory<SearchBot>
    {
        private readonly IMessagesSender messagesSender;
        private readonly IDataStorage<SearchBot> dataStorage;

        public SubFSMFactory(IMessagesSender messagesSender, IDataStorage<SearchBot> dataStorage)
        {
            this.dataStorage = dataStorage;
            this.messagesSender = messagesSender;
        }
        public async Task<ISubFSM<SearchBot>> Get(Update update)
        {
            if (Bot.Constants.CallSettings.Contains(update.Message.Text.ToLower()))
            {
                return new ConfigProcessor(messagesSender, dataStorage);
            }
            else return ConfigProcessor.Default;
        }
    }
}
