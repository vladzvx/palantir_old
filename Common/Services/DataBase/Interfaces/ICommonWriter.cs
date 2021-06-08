using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Services.Interfaces
{
    public interface ICommonWriter
    {
        public void PutData(object data);

        public int GetQueueCount();
    }
}
