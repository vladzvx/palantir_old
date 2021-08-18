using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
        public static Page Empty = new Page(new Guid(), 0) {position=Position.Empty };
        public Position position { get; set; }
        public Guid SearchGuid;
        public int Number;
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
            if (Text.Length < 3700)
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
            else return false;
        }

        public TextMessage GetTextMessage(ITelegramBotClient client, long chatId)
        {
            return new TextMessage(client, chatId, Text, null);
        }
    }
}
