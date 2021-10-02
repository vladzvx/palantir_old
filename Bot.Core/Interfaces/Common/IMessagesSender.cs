using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Interfaces
{
    public interface IMessagesSender
    {
        public void AddItem(ISendedItem sendedItem);
    }
}
