using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services.DataBase
{
    public class ConnectionWrapper : IDisposable
    {
        public readonly NpgsqlConnection Connection;
        public readonly ConnectionPoolManager connectionPoolManager;
        public readonly int Id;
        internal ConnectionWrapper(string ConnectionString, int Id, ConnectionPoolManager connectionPoolManager)
        {
            this.Id = Id;
            this.connectionPoolManager = connectionPoolManager;
            this.Connection = new NpgsqlConnection(ConnectionString);
        }
        public void Dispose()
        {
            connectionPoolManager.Pool.Add(this);
        }
    }
}
