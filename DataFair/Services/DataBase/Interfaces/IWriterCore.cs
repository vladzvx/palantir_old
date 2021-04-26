using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair.Services.Interfaces
{
    public interface IWriterCore<TData> where TData : class
    {
        public string ConnectionString { get; }
        public int TrasactionSize { get; }
        public DbCommand CommandCreator(DbConnection connection);
        public void WriteSingleObject(DbCommand command, TData data);
    }
}
