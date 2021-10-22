using Npgsql;
using System;

namespace Common.Services.DataBase
{
    public class ConnectionWrapper : IDisposable
    {
        public readonly NpgsqlConnection Connection;
        public readonly ConnectionsFactory connectionPoolManager;
        public DateTime CreationTime;
        internal ConnectionWrapper(string ConnectionString, ConnectionsFactory connectionPoolManager)
        {
            this.connectionPoolManager = connectionPoolManager;
            this.Connection = new NpgsqlConnection(ConnectionString);
            this.Connection.Disposed += Connection_Disposed1;
            CreationTime = DateTime.UtcNow;//Guid.NewGuid();
        }

        private void Connection_Disposed1(object sender, EventArgs e)
        {
            connectionPoolManager.PoolRepo.TryRemove(CreationTime, out var _);
        }
        public void Dispose()
        {
            if (DateTime.UtcNow.Subtract(this.CreationTime) > connectionPoolManager.settings.ConnectionLifetime && connectionPoolManager.Pool.Count>1)
            {
                Connection.Close();
                Connection.Dispose();
            }
            else
            {
                connectionPoolManager.Pool.Add(this);
            }

        }
    }
}