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
    public class Notifire : RabbitMQBaseConsumer
    {
        private readonly static Regex regex= new Regex(@"^(\d+):.+$");
        private readonly IMessagesSender messagesSender;
        public Notifire(IMessagesSender messagesSender, IRabbitMQSettings rabbitMQSettings, ConnectionFactory connectionFactory, IBotSettings botSettings) :
            base(rabbitMQSettings, connectionFactory, TrimToken(botSettings.Token))
        {
            this.messagesSender = messagesSender;
        }

        private static string TrimToken(string token)
        {
            Match mathc = regex.Match(token);
            if (mathc.Success)
            {
                return mathc.Groups[1].Value;
            }
            else return null;
        }
        public override void ConsumerReceived(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var qq = (NotiModel)System.Text.Json.JsonSerializer.Deserialize(Encoding.UTF8.GetString(e.Body.Span), typeof(NotiModel));
                if (qq.ChatId == 0)
                {
                    foreach (long key in Bot.Core.Services.Bot.FSM<Bot.Core.Models.ObserverBot>.Factory.state.Keys)
                    {
                        //messagesSender.AddItem(new TextMessage(null, key, qq.Link + "\n\n" + Math.Round(qq.Rank, 3) + "\n\n" + qq.Text, null));
                        messagesSender.AddItem(new TextMessage(null, key, qq.Link + "\n\n" + qq.Text, null));
                    }
                }
                else
                {
                    //messagesSender.AddItem(new TextMessage(null, qq.ChatId, qq.Link + "\n\n" + Math.Round(qq.Rank, 3) + "\n\n" + qq.Text, null));
                    messagesSender.AddItem(new TextMessage(null, qq.ChatId, qq.Link + "\n\n" + qq.Text, null));
                }
            }
            catch (Exception ex)
            {
                listeningConnection.Dispose();
                listeningChannel.Dispose();
                Connect();
            }

        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await Bot.Core.Services.Bot.FSM<Bot.Core.Models.ObserverBot>.Factory.Load(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
        }
    }
}
