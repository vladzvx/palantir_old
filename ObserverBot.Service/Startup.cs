using Bot.Core;
using Bot.Core.Interfaces;
using Bot.Core.Services;
using Bot.Service.Services;
using Common.Services;
using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Common.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ObserverBot.Service.Services;
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
            services.AddSingleton<ConnectionsFactory>();
            services.AddSingleton<IBotSettings, BotSettings>();
            services.AddSingleton(new CancellationTokenSource());
            services.AddHostedService<BotsEntryPoint>();
            services.AddHostedService<Notifire>();
            services.AddSingleton<DBWorker>();
            services.AddSingleton<ISenderSettings, SenderSettings>();
            services.AddSingleton<IMessagesSender, MessagesSender>();
            services.AddTransient<Bot.Core.Services.Bot>();
            services.AddSingleton<ISubFSM, ConfigurationProcessor>();
            services.AddSingleton<IReadyProcessor, EmptyReadyProcessor>();
            services.AddSingleton<ICommonWriter<Message>, CommonWriter<Message>>();
            services.AddSingleton<IWriterCore<Message>, BotMessagesWriterCore>();
            services.AddTransient<IDataBaseSettings, DataBaseSettings>();
            services.AddSingleton<IStartedProcessor, ObsStartedProcessor>();
            //services.AddSingleton<AsyncTaskExecutor>();
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
