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
            ConfigurationContainer cont = new ConfigurationContainer() { Session = new Common.SessionSettings() { SQLDialect = "ssss" } };
            await serverStream.WriteAsync(cont);
            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {

            }
        }
    }
}
