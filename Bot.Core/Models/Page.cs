using Common;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Models
{
    public class Page
    {
        public const int PageMaxSize = 1500;
        public const int PreviewSize = 150;
        public static Page Empty = new Page(ObjectId.Empty) { position = Position.Empty };


        public enum Position
        {
            First,
            Middle,
            Last,
            Single,
            Empty
        }

        public Position position { get; set; }
        public ObjectId Id { get; private set; }
        public ObjectId? NextId { get; private set; }
        public ObjectId? PreviousId { get; private set; }

        public int Number;
        public string Text { get; set; } = string.Empty;
        public List<MessageEntity> Formatting { get; set; } = new List<MessageEntity>();
        public int offset { get; set; } = 0;
        public int count { get; set; } = 0;



        public List<SearchResult> results = new List<SearchResult>();
        public Page(ObjectId id, ObjectId? previousId=null, ObjectId? nextId=null)
        {
            this.Id = id;
            this.NextId = nextId;
            this.PreviousId = previousId;
        }
        public bool TryAddResult(SearchResult res, int pageSize= PageMaxSize, int previewSize = PreviewSize)
        {
            if (Text.Length < pageSize)
            {
                string link = !string.IsNullOrEmpty(res.Name) ? res.Name :"Ссылка №" +count.ToString();
                string line = link + ":\n" + (res.Text.Length > previewSize ? res.Text.Substring(0, previewSize - 1) + "..." : res.Text) + "\n\n";

                MessageEntity formatting = new MessageEntity() { Length = link.Length+1, Url = res.Link, Offset = offset, Type = MessageEntityType.TextLink };
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

        public TextMessage GetTextMessage(ITelegramBotClient client, long chatId, Channel<int> channel=null, bool firstPage = false)
        {
            InlineKeyboardButton donate = new InlineKeyboardButton();
            donate.Text = "Поддержать проект";
            donate.Url = Environment.GetEnvironmentVariable("donate")??"https://new.donatepay.ru/@yesod";

            InlineKeyboardButton Empty = new InlineKeyboardButton();
            Empty.Text = " - ";
            Empty.CallbackData = "-";

            InlineKeyboardButton Next = new InlineKeyboardButton();
            Next.Text = "Далее";
            Next.CallbackData = NextId.HasValue ? NextId.Value.ToString() : string.Empty;

            InlineKeyboardButton Prev = new InlineKeyboardButton();
            Prev.Text = "Назад";
            Prev.CallbackData = PreviousId.HasValue ? PreviousId.Value.ToString() : string.Empty;

            if ((Text.Length <= PageMaxSize) && !(position == Position.Last))
            {
                position = Position.Single;
            }

            InlineKeyboardMarkup keyb;
            if (position == Position.First)
            {
                keyb = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>(){ new List<InlineKeyboardButton> { Next },
                    new List<InlineKeyboardButton>() { donate}});
            }
            else if (position == Position.Middle)
            {
                keyb = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>(){ new List<InlineKeyboardButton> { Prev, Next },
                    new List<InlineKeyboardButton>() { donate}});
            }
            else if (position == Position.Last)
            {
                keyb = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>(){ new List<InlineKeyboardButton> { Prev, },
                    new List<InlineKeyboardButton>() { donate}});
            }
            else if (position==Position.Single)
            {
                keyb = new InlineKeyboardMarkup(donate);
            }
            else
            {
                keyb = null;
            }

            if (firstPage)
            {
                keyb = new InlineKeyboardMarkup(Empty);
            }
            return new TextMessage(client, chatId, Text, channel, new ReplyKeyboardRemove(), formattings: new List<MessageEntity>(Formatting),
                inlineKeyboard: keyb);
        }

        public EditTextMessage GetEditTextMessage(ITelegramBotClient client, long chatId,int messageId, Channel<int> channel = null)
        {
            InlineKeyboardButton donate = new InlineKeyboardButton();
            donate.Text = "Поддержать проект";
            string donateLink = Environment.GetEnvironmentVariable("donate");
            if (donateLink != null)
            {
                donate.Url = donateLink;
            }
            else
            {
                donate.CallbackData = "Donate";
            }

            InlineKeyboardButton Next = new InlineKeyboardButton();
            Next.Text = "Далее";
            Next.CallbackData = NextId.HasValue ? NextId.Value.ToString() : string.Empty;
            
            InlineKeyboardButton Prev = new InlineKeyboardButton();
            Prev.Text = "Назад";
            Prev.CallbackData = PreviousId.HasValue ? PreviousId.Value.ToString() : string.Empty;

            InlineKeyboardMarkup keyb;


            if (position == Position.First)
            {
                keyb = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>(){ new List<InlineKeyboardButton> { Next },
                    new List<InlineKeyboardButton>() { donate}});
            }
            else if (position == Position.Middle)
            {
                keyb = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>(){ new List<InlineKeyboardButton> { Prev, Next },
                    new List<InlineKeyboardButton>() { donate}});
            }
            else if (position == Position.Last)
            {
                keyb = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>(){ new List<InlineKeyboardButton> { Prev, },
                    new List<InlineKeyboardButton>() { donate}});
            }
            else if (position == Position.Single)
            {
                keyb = new InlineKeyboardMarkup(donate);
            }
            else
            {
                keyb = null;
            }
            return new EditTextMessage(client, chatId, Text, messageId, channel, new ReplyKeyboardRemove(), formattings: new List<MessageEntity>(Formatting),
                inlineKeyboard: keyb);
        }
    }
}
