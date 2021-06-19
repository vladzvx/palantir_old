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
        public readonly Guid Id;
        internal ConnectionWrapper(string ConnectionString, ConnectionPoolManager connectionPoolManager)
        {
            this.connectionPoolManager = connectionPoolManager;
            this.Connection = new NpgsqlConnection(ConnectionString);
            this.Connection.Disposed += Connection_Disposed1;
            Id = new Guid();
        }

        private void Connection_Disposed1(object sender, EventArgs e)
        {
            connectionPoolManager.PoolRepo.TryRemove(Id, out var _);
        }

        public void Dispose()
        {
            connectionPoolManager.Pool.Add(this);
        }
    }
}
