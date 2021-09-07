using Bot.Core;
using Bot.Core.Interfaces;
using Bot.Core.Models;
using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Microsoft.Extensions.Hosting;
using Npgsql;
using ObserverBot.Service.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ObserverBot.Service
{
    public class Notifire : IHostedService
    {
        private Task waitingTask;
        private readonly IMessagesSender messagesSender;
        private readonly ConnectionsFactory connectionsFactory;
        public Notifire(IMessagesSender messagesSender)
        {
            DataBaseSettingsObserver settings = new DataBaseSettingsObserver();
            this.messagesSender = messagesSender;
            connectionsFactory = new ConnectionsFactory(settings);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Bot.Core.Services.Bot.FSM.Factory.Load(TextMessage.defaultClient);
            var connectionWr= await connectionsFactory.GetConnectionAsync(CancellationToken.None);
            var connection = connectionWr.Connection;
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
            foreach (long key in Bot.Core.Services.Bot.FSM.Factory.state.Keys)
            {
                messagesSender.AddItem(new TextMessage(null, key, e.Payload, null));
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
