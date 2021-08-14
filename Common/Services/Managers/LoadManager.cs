using Common.Services.DataBase.Interfaces;
using Common.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services.Managers
{
    public class LoadManager : ILoadManager
    {
        private readonly ICommonWriter<Message> commonWriter;
        public LoadManager(ICommonWriter<Message> commonWriter)
        {
            this.commonWriter = commonWriter;
        }
        public bool CheckPauseNecessity()
        {
            return commonWriter.GetQueueCount() > 150000;
        }
    }
}
