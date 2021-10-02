using Common.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public class MongoWriter : ICommonWriter<Update>
    {
        public async Task ExecuteAdditionalAction(object data)
        {
            //throw new NotImplementedException();
        }

        public int GetQueueCount()
        {
            return 0;
        }

        public void PutData(Update data)
        {

        }
    }
}
