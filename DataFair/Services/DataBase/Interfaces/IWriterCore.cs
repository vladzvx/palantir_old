using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair.Services.Interfaces
{
    public interface IWriterCore
    {
        public TimeSpan ReconnerctionPause { get; }
        public double StartWritingInterval { get; }
        public string ConnectionString { get; }
        public int TrasactionSize { get; }
        public DbCommand CreateCommand(DbConnection connection,Type dataType);
        public void WriteSingleObject(object data, DbTransaction transaction);
    }
}
