using Bot.Core;
using Bot.Core.Interfaces;
using Bot.Core.Interfaces.BotFSM;
using Bot.Core.Services;
using Common;
using Common.Interfaces;
using Common.Services;
using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Common.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using ObserverBot.Service.Services;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ObserverBot.Service
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new CancellationTokenSource());
            services.AddSingleton<IBotSettings, BotSettings>();

            services.AddHostedService<BotsEntryPoint<Bot.Core.Models.ObserverBot>>();
            services.AddSingleton<ISenderSettings, SenderSettings>();
            services.AddSingleton<IMessagesSender, MessagesSender>();
            services.AddSingleton<ICommonWriter<Update>, MongoWriter>();
            services.AddSingleton<IDataStorage<Bot.Core.Models.ObserverBot>, DataStorage<Bot.Core.Models.ObserverBot>>();
            services.AddSingleton(new MongoClient(Options.MongoConnectionString));
            services.AddSingleton<IDataStorage<Bot.Core.Models.ObserverBot>, DataStorage<Bot.Core.Models.ObserverBot>>();

            services.AddTransient<IFSMFactory<Bot.Core.Models.ObserverBot>, ObserverSubFSMFactory>();
            services.AddTransient<IReadyProcessor<Bot.Core.Models.ObserverBot>, ObserverReadyProcessor>();
            services.AddTransient<IBusyProcessor, BusyProcessor>();
            services.AddTransient<IRightChecker, RightChecker>();

            services.AddSingleton<AsyncTaskExecutor>();
            services.AddTransient<Bot.Core.Services.Bot>();


            services.AddSingleton<ConnectionFactory>();
            services.AddTransient<IRabbitMQSettings, RabbitMQSettings>();
            services.AddHostedService<Notifire>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
        }
    }
}
