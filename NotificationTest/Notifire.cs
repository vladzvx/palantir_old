using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationTest
{
    public class Notifire : IHostedService
    {
        private Task waitingTask;
        private readonly NpgsqlConnection connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("ConnectionString"));
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await connection.OpenAsync();
            DbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = "listen test;";
            await dbCommand.ExecuteNonQueryAsync();
            connection.Notification += Connection_Notification;
            waitingTask = Task.Factory.StartNew(() => 
            { 
                while (true)
                {
                    connection.Wait();
                }
            },TaskCreationOptions.LongRunning);
        }

        private void Connection_Notification(object sender, NpgsqlNotificationEventArgs e)
        {
            Console.WriteLine(e.Payload);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
