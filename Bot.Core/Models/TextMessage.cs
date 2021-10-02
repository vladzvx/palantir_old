using Bot.Core.Interfaces;
using Common.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Models
{
    public class TextMessage : ISendedItem
    {
        public static ITelegramBotClient defaultClient;
        public static ICommonWriter<Message> commonWriter;
        internal readonly ITelegramBotClient client;
        internal readonly IReplyMarkup keyboard;
        internal readonly InlineKeyboardMarkup keyboard2;
        internal readonly int replyToMessageId;
        internal readonly string Text;
        internal readonly IEnumerable<MessageEntity> Formatting;
        public long ChatId { get; private set; }

        public ChannelReader<int> Ready { get; private set; }
        public ChannelWriter<int> ReadyWriter;

        public TextMessage(ITelegramBotClient client, long chatId, string text, Channel<int> channel, IReplyMarkup keyboard = null, int replyToMessageId = 0, IEnumerable<MessageEntity> formattings = null, InlineKeyboardMarkup inlineKeyboard = null)
        {
            this.client = client ?? defaultClient;
            this.keyboard = keyboard;
            this.keyboard2 = inlineKeyboard;
            this.ChatId = chatId;
            this.Text = text;
            this.replyToMessageId = replyToMessageId;
            this.Formatting = formattings;
            if (channel != null)
            {
                Ready = channel.Reader;
                ReadyWriter = channel.Writer;
            }
        }
        public virtual async Task Send()
        {
            Message result = null;
            try
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    result = await client.SendTextMessageAsync(ChatId, Text, replyMarkup: keyboard2 ?? keyboard, replyToMessageId: replyToMessageId, entities: Formatting);

                    commonWriter?.PutData(result);
                }
            }
            catch (Exception ex) 
            { 
            
            }
            if (ReadyWriter != null && result != null)
            {
                await ReadyWriter.WriteAsync(result.MessageId);
            }
        }
    }
}
