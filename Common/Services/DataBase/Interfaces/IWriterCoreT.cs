using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.Interfaces
{
    public interface IWriterCore<T>
    {
        public DbCommand CreateMainCommand(DbConnection dbConnection);
        public DbCommand CreateAdditionalCommand(DbConnection dbConnection)
        {
            return null;
        }
        public Task AdditionaAcion(object data);
        public Task Write(T data, CancellationToken token);


    }
}
