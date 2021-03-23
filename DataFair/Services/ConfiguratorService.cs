using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;
using System.Timers;
using System.Threading;
using Timer = System.Timers.Timer;

namespace DataFair.Services
{
    public class ConfiguratorService :Common.Configurator.ConfiguratorBase
    {
        public async override Task GetConfiguration(Empty req, IServerStreamWriter<ConfigurationContainer> serverStream, ServerCallContext context)
        {
            int qq = 0;
            SessionSettings session;
            User user;
            Collector collector;
            if (Storage.Sessions.TryPeek(out session)&& 
                Storage.Users.TryTake(out user) && 
                Storage.Collectors.TryPeek(out collector))
            {
                ConfigurationContainer cont = new ConfigurationContainer()
                {
                    Session = session, CollectorParams=collector,UserParams=user
                };
                await serverStream.WriteAsync(cont);
                try
                {
                    await Task.Delay(-1, context.CancellationToken);
                }
                catch (TaskCanceledException)
                {
                    Storage.Users.Add(user);
                }
            }
        }
    }
}
