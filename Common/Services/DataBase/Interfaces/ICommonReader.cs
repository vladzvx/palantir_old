using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase.Interfaces
{
    public interface ICommonReader<T> where T: class
    {
        public Task<T> ReadAsync(object parameters, CancellationToken cancellationToken);
    }
}
