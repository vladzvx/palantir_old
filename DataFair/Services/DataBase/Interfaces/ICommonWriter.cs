using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair.Services.Interfaces
{
    public interface ICommonWriter<TData> where TData : class
    {
        public void PutData(TData data);

        public int GetQueueCount();
    }
}
