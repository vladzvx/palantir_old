using System;
using System.Data.Common;

namespace Common.Services.Interfaces
{
    public interface IWriterCore
    {
        public TimeSpan ReconnerctionPause { get; }
        public double StartWritingInterval { get; }
        public string ConnectionString { get; }
        public int TrasactionSize { get; }
        public DbCommand CreateCommand(DbConnection connection, Type dataType);
        public void WriteSingleObject(object data, DbTransaction transaction);
    }
}
