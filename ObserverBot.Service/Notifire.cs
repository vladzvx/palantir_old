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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ObserverBot.Service
{
    public class Notifire : IHostedService
    {
        private Regex GetLinkReg = new Regex("\"link\":(.+);");
        private Regex GetTextReg = new Regex("\"text\": (.+)\"client\":");
        private Regex GetUser = new Regex("\"client\":(\\d+)\"requester\"");
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
            Match user = GetUser.Match(e.Payload);
            Match link = GetLinkReg.Match(e.Payload);
            Match text = GetTextReg.Match(e.Payload);
            if (user.Success && link.Success && text.Success && long.TryParse(user.Groups[1].Value, out long id))
            {
                string txt = string.Format("{0}\n\n{1}", link.Groups[1].Value, text.Groups[1].Value);
                if (id == 0)
                {
                    foreach (long key in Bot.Core.Services.Bot.FSM.Factory.state.Keys)
                    {
                        messagesSender.AddItem(new TextMessage(null, key, txt, null));
                    }
                }
                else
                {
                    messagesSender.AddItem(new TextMessage(null, id, txt, null));
                }

            }

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
