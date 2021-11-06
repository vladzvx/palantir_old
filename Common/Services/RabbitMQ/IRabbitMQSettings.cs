using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IRabbitMQSettings
    {
        public string BrokerName { get => Environment.GetEnvironmentVariable("RabbitMQ_BrokerName")??"notifications_exchanger"; } 
        public string ExchangeType { get => "direct"; }
        public string RoutingKey { get => "all"; }
        //public string AutofacScopeName { get; set; }
        public string ExchangeName { get => Environment.GetEnvironmentVariable("RabbitMQ_ExchangeName")??"noti_ex"; }
        public string QueueName { get => Environment.GetEnvironmentVariable("RabbitMQ_QueueName")??"111"; }
        //public int RetryCount { get; set; }
        public string VirtualHost { get => Environment.GetEnvironmentVariable("RabbitMQ_VirtualHost") ?? "/"; }
        public string UserName { get => Environment.GetEnvironmentVariable("RabbitMQ_UserName");  }
        public string Password { get => Environment.GetEnvironmentVariable("RabbitMQ_Password");  }
        public string HostName { get => Environment.GetEnvironmentVariable("RabbitMQ_HostName"); }
        public int Port { get => int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port")); }
        //public int NetworkRecoveryInterval { get; set; }
        //public int RequestedHeartbeat { get; set; }
        //public bool DispatchConsumersAsync { get; set; }

        public static void ApplySettings(IRabbitMQSettings rabbitMQSettings, ConnectionFactory connectionFactory)
        {
            connectionFactory.HostName = rabbitMQSettings.HostName;
            connectionFactory.UserName = rabbitMQSettings.UserName;
            connectionFactory.Password = rabbitMQSettings.Password;
            //connectionFactory.Q = rabbitMQSettings.QueueName;
            connectionFactory.Port = rabbitMQSettings.Port;
            connectionFactory.VirtualHost = rabbitMQSettings.VirtualHost;
            
        }
    }
}
