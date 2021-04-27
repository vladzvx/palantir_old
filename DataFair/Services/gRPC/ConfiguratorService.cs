﻿using Grpc.Core;
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
        private readonly State state;
        public ConfiguratorService(State state)
        {
            this.state = state;
        }
        public async override Task GetConfiguration(Empty req, IServerStreamWriter<ConfigurationContainer> serverStream, ServerCallContext context)
        {
            SessionSettings session;
            Collector collector;
            if (state.SessionStorages.TryPeek(out session)&&
                state.Collectors.TryTake(out collector))
            {
                ConfigurationContainer cont = new ConfigurationContainer()
                {
                    Session = session, CollectorParams=collector
                };
                await serverStream.WriteAsync(cont);
                try
                {
                    await Task.Delay(-1, context.CancellationToken);
                }
                catch (TaskCanceledException)
                {
                    state.Collectors.Add(collector);
                }
            }
        }
    }
}