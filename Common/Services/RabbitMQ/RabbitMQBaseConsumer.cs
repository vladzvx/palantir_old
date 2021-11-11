using Common.Interfaces;
using Common.Models;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services
{
    public class RabbitMQBaseConsumer :  IHostedService
    {
        protected readonly EventingBasicConsumer consumer;
        protected readonly IConnection listeningConnection;
        protected readonly IModel listeningChannel;
        protected readonly ConnectionFactory connectionFactory;
        protected readonly IRabbitMQSettings rabbitMQSettings;


        public RabbitMQBaseConsumer(IRabbitMQSettings rabbitMQSettings, ConnectionFactory connectionFactory, string ExchangeId=null)
        {
            IRabbitMQSettings.ApplySettings(rabbitMQSettings, connectionFactory);

            this.connectionFactory = connectionFactory;
            this.rabbitMQSettings = rabbitMQSettings;
            listeningConnection = connectionFactory.CreateConnection();
            listeningChannel = listeningConnection.CreateModel();
            listeningChannel.ExchangeDeclare(ExchangeId ?? this.rabbitMQSettings.ExchangeName, rabbitMQSettings.ExchangeType, true, false);
            listeningChannel.QueueDeclare(this.rabbitMQSettings.QueueName, true, false, true);
            listeningChannel.BasicQos(0, 0, false);
            listeningChannel.QueueBind(this.rabbitMQSettings.QueueName, ExchangeId ?? this.rabbitMQSettings.ExchangeName,
                rabbitMQSettings.RoutingKey, new Dictionary<string, object>());
            consumer = new EventingBasicConsumer(listeningChannel);
            consumer.Received += ConsumerReceived;

            listeningChannel.BasicConsume(
                queue: this.rabbitMQSettings.QueueName,
                autoAck: false,
                consumer: consumer
                );
        }

        public virtual void Publish(ReadOnlyMemory<byte> body,string exchange)
        {
            listeningChannel.BasicPublish(exchange, rabbitMQSettings.RoutingKey, null, body);
        }
        public virtual void ConsumerReceived(object sender, BasicDeliverEventArgs e)
        {
            var qq = System.Text.Json.JsonSerializer.Deserialize(Encoding.UTF8.GetString(e.Body.Span), typeof(NotiModel));


        }
        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {

        }
    }
}
