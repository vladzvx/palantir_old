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
        public Task ExecuteAdditionaAcion(DbCommand command, object data, CancellationToken token);
        public Task ExecuteWriting(DbCommand command, T data, CancellationToken token);
    }
}
