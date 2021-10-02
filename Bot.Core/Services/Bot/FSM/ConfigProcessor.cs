using Bot.Core.Enums;
using Bot.Core.Interfaces;
using Bot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Bot.Core.Services.Bot;

namespace Bot.Core.Services
{
    public class ConfigProcessor: ISubFSM<SearchBot>
    {

        private ConfiguringSubstates state;
        private readonly IMessagesSender messagesSender;
        private readonly IDataStorage<SearchBot> dataStorage;

        public static ConfigProcessor Default = new ConfigProcessor(null, null) { IsEmpty = true };
        public bool IsEmpty { get; set; } = false;

        public ConfigProcessor(IMessagesSender messagesSender, IDataStorage<SearchBot> dataStorage)
        {
            this.messagesSender = messagesSender;
            this.dataStorage = dataStorage;
        }
        public async Task<bool> ProcessUpdate(Update update, SearchBot parentFSM, CancellationToken token)
        {
            switch (state)
            {
                case ConfiguringSubstates.Started:
                    {
                        TextMessage mess = new TextMessage(null, update.Message.Chat.Id, "Выберите глубину поиска", Channel.CreateBounded<int>(1), keyboard: Constants.Keyboards.settingKeyboard);
                        messagesSender.AddItem(mess);
                        state = ConfiguringSubstates.ConfiguringDepth;
                        return false;
                    }
                case ConfiguringSubstates.ConfiguringDepth:
                    {
                        parentFSM.RequestDepth = ParseDepth(update);
                        ISendedItem mess = Bot.Common.CreateOk(update, Constants.Keyboards.yesNoKeyboard, " Искать в группах?");
                        messagesSender.AddItem(mess);
                        state = ConfiguringSubstates.ConfiguringGroups;
                        return false;
                    }
                case ConfiguringSubstates.ConfiguringGroups:
                    {
                        parentFSM.SearchInGroups = update.Message.Text.ToLower() == "да";
                        ISendedItem mess = Bot.Common.CreateOk(update, Constants.Keyboards.yesNoKeyboard, " Искать в каналах?");
                        messagesSender.AddItem(mess);
                        state = ConfiguringSubstates.ConfiguringChannel;
                        return false;
                    }
                case ConfiguringSubstates.ConfiguringChannel:
                    {
                        parentFSM.SearchInChannels = update.Message.Text.ToLower() == "да";
                        ISendedItem mess = Bot.Common.CreateOk(update, new ReplyKeyboardRemove(), " Настройки завершены. Для поиска просто отправьте слово/словосочетание боту.");
                        parentFSM.BotState = PrivateChatState.Ready;
                        messagesSender.AddItem(mess);
                        await dataStorage.SaveChat(parentFSM, token);
                        //current.Finished = true;
                        state = ConfiguringSubstates.Started;
                        return true;
                    }
                default:
                    {
                        return true;
                    }
            }
        }

        private RequestDepth ParseDepth(Update update)
        {
            string tmp = update.Message.Text.ToLower();
            if (Constants.Day.Contains(tmp))
            {
                return RequestDepth.Day;
            }
            else if (Constants.Week.Contains(tmp))
            {
                return RequestDepth.Week;
            }
            else if (Constants.Month.Contains(tmp))
            {
                return RequestDepth.Month;
            }
            else if (Constants.Year.Contains(tmp))
            {
                return RequestDepth.Year;
            }
            else
            {
                return RequestDepth.Inf;
            }

        }
    }
}
