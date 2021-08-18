using Bot.Core.Interfaces;
using Common.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Models
{
    public class TextMessage : ISendedItem
    {
        public static ICommonWriter<Message> commonWriter;
        private readonly ITelegramBotClient client;
        private readonly IReplyMarkup keyboard;
        private readonly int replyToMessageId;
        private readonly string Text;
        public long ChatId { get; private set; }

        public ChannelReader<bool> Ready { get; private set; }
        private ChannelWriter<bool> ReadyWriter;

        public TextMessage(ITelegramBotClient client, long chatId, string text, Channel<bool> channel, IReplyMarkup keyboard=null, int replyToMessageId=0)
        {
            this.client = client;
            this.keyboard = keyboard;
            this.ChatId = chatId;
            this.Text = text;
            this.replyToMessageId = replyToMessageId;
            if (channel != null)
            {
                Ready = channel.Reader;
                ReadyWriter = channel.Writer;
            }
        }
        public async Task Send()
        {
            bool res = false;
            try
            {
                Message result = await client.SendTextMessageAsync(ChatId, Text, replyMarkup: keyboard, replyToMessageId: replyToMessageId);
                commonWriter?.PutData(result);
                res = true;
            }
            catch { }
            if (ReadyWriter != null)
                await ReadyWriter.WriteAsync(res);
        }
    }
}
