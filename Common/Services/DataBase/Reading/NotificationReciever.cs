using Common.Models;
using Common.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase
{
    public class NotificationReciever : IHostedService
    {
        private Thread workerThread;
        private readonly ConnectionsFactory connectionsFactory;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly RabbitMQBase rabbitMQBase;
        public NotificationReciever(ConnectionsFactory connectionsFactory, RabbitMQBase rabbitMQBase)
        {
            this.connectionsFactory = connectionsFactory;
            this.rabbitMQBase = rabbitMQBase;
        }

        public void Worker(object token)
        {
            if (token is CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        using (var connectionWr = connectionsFactory.GetConnectionAsync(CancellationToken.None).Result)
                        {
                            var connection = connectionWr.Connection;
                            DbCommand dbCommand = connection.CreateCommand();
                            dbCommand.CommandText = "listen test;";
                            dbCommand.ExecuteNonQuery();
                            connection.Notification += Connection_Notification;
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                connection.Wait();
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    Thread.Sleep(1000);
                }
            }
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            workerThread = new Thread(new ParameterizedThreadStart(Worker));
            workerThread.Start(cts.Token);
        }

        private void Connection_Notification(object sender, NpgsqlNotificationEventArgs e)
        {
            try
            {
                var q = System.Text.Json.JsonSerializer.Deserialize(e.Payload, typeof(NotiModel));
                if (q is NotiModel notiModel)
                    rabbitMQBase.Publish(Encoding.UTF8.GetBytes(e.Payload), notiModel.BotId.ToString());
            }
            catch (Exception ex)
            {

            }

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
