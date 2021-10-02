using Bot.Core.Interfaces;
using Bot.Core.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Channels;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Services
{
    public partial class Bot
    {
        public static class Common
        {
            public static ISendedItem CreateImBusy(Update update)
            {
                return new TextMessage(null, update.Message.Chat.Id, Constants.BusyMessage, Channel.CreateBounded<int>(1), keyboard: Constants.Keyboards.searchingKeyboard);
            }
            public static ISendedItem CreateOk(Update update, IReplyMarkup keyboard = null, string additionalMessage = "")
            {
                return new TextMessage(null, update.Message.Chat.Id, Constants.OkMessage + additionalMessage, Channel.CreateBounded<int>(1), keyboard, replyToMessageId: update.Message.MessageId);
            }
        }
    }
}
