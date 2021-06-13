using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Services.Interfaces
{
    public interface ICommonProcessor<T>
    {
        public void Process(T data);

        public bool IsResultsOk { get;}
        public int ProcessingsCounter { get;}

    }
}
