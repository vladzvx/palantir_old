using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class NotiModel
    {
        public double Rank { get; set; }
        public string Link { get; set; }
        public string Text { get; set; }
        public long BotId { get; set; }
        public long ChatId { get; set; }
    }
}
