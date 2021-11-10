using Bot.Core;
using Bot.Core.Interfaces;
using Bot.Core.Models;
using Common.Interfaces;
using Common.Models;
using Common.Services;
using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Microsoft.Extensions.Hosting;
using Npgsql;
using ObserverBot.Service.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ObserverBot.Service
{
    public class Notifire : RabbitMQBase
    {
        private readonly IMessagesSender messagesSender;
        public Notifire(IMessagesSender messagesSender, IRabbitMQSettings rabbitMQSettings, ConnectionFactory connectionFactory):base(rabbitMQSettings,connectionFactory)
        {
            this.messagesSender = messagesSender;
        }

        public override void ConsumerReceived(object sender, BasicDeliverEventArgs e)
        {
            var qq = (NotiModel)System.Text.Json.JsonSerializer.Deserialize(Encoding.UTF8.GetString(e.Body.Span), typeof(NotiModel));
            if (qq.ChatId == 0)
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
        }
    }
}
