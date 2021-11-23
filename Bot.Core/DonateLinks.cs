using Bot.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core
{
    public static class DonateLinks
    {
        public static ConcurrentDictionary<string, LinkInfo> keyboards = new ConcurrentDictionary<string, LinkInfo>();
        static DonateLinks()
        {
            keyboards.TryAdd("QIWI", new LinkInfo() {Name="QIWI",Data="",Link= "qiwi.com/n/FUTURISM" });
            keyboards.TryAdd("YooMoney", new LinkInfo() {Name= "YooMoney", Data="",Link= "https://sobe.ru/na/futurizm" });
            keyboards.TryAdd("Donatepay", new LinkInfo() {Name= "Donatepay", Data="",Link= "https://new.donatepay.ru/@yesod" });
        }
    }
}
