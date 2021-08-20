using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Models
{
    public class EditTextMessage : TextMessage
    {
        private readonly int messageId;
        public EditTextMessage(ITelegramBotClient client, long chatId, string text, int messageId, Channel<int> channel, IReplyMarkup keyboard = null, int replyToMessageId = 0, IEnumerable<MessageEntity> formattings = null, InlineKeyboardMarkup inlineKeyboard = null) : base(client, chatId, text, channel, keyboard, replyToMessageId, formattings, inlineKeyboard)
        {
            this.messageId = messageId;
        }
        public override async Task Send()
        {
            try
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    Message result = await client.EditMessageTextAsync(ChatId, messageId, Text, replyMarkup: keyboard2, entities: Formatting);
                    commonWriter?.PutData(result);
                }
            }
            catch { }
            if (ReadyWriter != null)
            {
                await ReadyWriter.WriteAsync(0);
            }
        }
    }
}
