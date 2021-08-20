using Common;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Models
{
    public class Page
    {
        public enum Position
        {
            First,
            Middle,
            Last,
            Empty
        }
        public static Page Empty = new Page(new Guid(), 0) { position = Position.Empty };
        public Position position { get; set; }
        public Guid SearchGuid;
        public int Number;
        public int MessageNumber { get; set; }
        public string Text { get; set; } = string.Empty;
        public List<MessageEntity> Formatting { get; set; } = new List<MessageEntity>();
        public int offset { get; set; } = 0;
        public int count { get; set; } = 0;

        public List<SearchResult> results = new List<SearchResult>();
        public Page(Guid SearchGuid, int Number)
        {
            this.SearchGuid = SearchGuid;
            this.Number = Number;
        }
        public bool TryAddResult(SearchResult res)
        {
            if (Text.Length < 1500)
            {
                string line = count.ToString() + ". " + (res.Text.Length > 100 ? res.Text.Substring(0, 99) + "..." : res.Text) + "\n\n";
                MessageEntity formatting = new MessageEntity() { Length = count.ToString().Length, Url = res.Link, Offset = offset, Type = MessageEntityType.TextLink };
                offset += line.Length;
                count++;
                Text += line;
                Formatting.Add(formatting);
                res.Page = this.Number;
                return true;
            }
            else
            {
                return false;
            }
        }

        public TextMessage GetTextMessage(ITelegramBotClient client, long chatId, Channel<int> channel = null)
        {
            InlineKeyboardButton Next = new InlineKeyboardButton();
            Next.Text = "Далее";
            Next.CallbackData = SearchGuid.ToString() + "_" + (Number + 1).ToString();

            InlineKeyboardButton Prev = new InlineKeyboardButton();
            Prev.Text = "Назад";
            Prev.CallbackData = SearchGuid.ToString() + "_" + (Number - 1).ToString();

            InlineKeyboardMarkup keyb;
            if (position == Position.First)
            {
                keyb = new InlineKeyboardMarkup(Next);
            }
            else if (position == Position.Middle)
            {
                keyb = new InlineKeyboardMarkup(new InlineKeyboardButton[2] { Prev, Next });
            }
            else if (position == Position.Last)
            {
                keyb = new InlineKeyboardMarkup(Prev);
            }
            else
            {
                keyb = null;
            }
            return new TextMessage(client, chatId, Text, channel, new ReplyKeyboardRemove(), formattings: Formatting,
                inlineKeyboard: keyb);
        }

        public EditTextMessage GetEditTextMessage(ITelegramBotClient client, long chatId, Channel<int> channel = null)
        {
            InlineKeyboardButton Next = new InlineKeyboardButton();
            Next.Text = "Далее";
            Next.CallbackData = SearchGuid.ToString() + "_" + (Number + 1).ToString();

            InlineKeyboardButton Prev = new InlineKeyboardButton();
            Prev.Text = "Назад";
            Prev.CallbackData = SearchGuid.ToString() + "_" + (Number - 1).ToString();

            InlineKeyboardMarkup keyb;
            if (position == Position.First)
            {
                keyb = new InlineKeyboardMarkup(Next);
            }
            else if (position == Position.Middle)
            {
                keyb = new InlineKeyboardMarkup(new InlineKeyboardButton[2] { Prev, Next });
            }
            else if (position == Position.Last)
            {
                keyb = new InlineKeyboardMarkup(Prev);
            }
            else
            {
                keyb = null;
            }
            return new EditTextMessage(client, chatId, Text, MessageNumber, channel, new ReplyKeyboardRemove(), formattings: Formatting,
                inlineKeyboard: keyb);
        }
    }
}
