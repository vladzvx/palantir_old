using Bot.Core.Interfaces;
using Bot.Core.Interfaces.BotFSM;
using Bot.Core.Services;
using Common;
using Common.Interfaces;
using Common.Services;
using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using GateKeeper.Service.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GateKeeper.Service
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ConventionRegistry.Register("IgnoreIfNullConvention", new ConventionPack { new IgnoreIfNullConvention(true) }, t => true);
            services.AddControllers();
            services.AddSingleton(new CancellationTokenSource());
            services.AddSingleton<IBotSettings, BotSettings>();

            services.AddHostedService<BotsEntryPoint<Bot.Core.Models.GateKeeperBot>>();
            services.AddSingleton<ISenderSettings, SenderSettings>();
            services.AddSingleton<IMessagesSender, MessagesSender>();
            services.AddSingleton<MongoWriter>();
            services.AddSingleton<SearchState< Bot.Core.Models.GateKeeperBot >> ();
            services.AddSingleton<IDataStorage<Bot.Core.Models.GateKeeperBot>, DataStorage<Bot.Core.Models.GateKeeperBot>>();
            services.AddSingleton(new MongoClient(Options.MongoConnectionString));
            services.AddSingleton<IDataStorage<Bot.Core.Models.GateKeeperBot>, DataStorage<Bot.Core.Models.GateKeeperBot>>();

            services.AddTransient<IFSMFactory<Bot.Core.Models.GateKeeperBot>, EmptySubFSMFactory<Bot.Core.Models.GateKeeperBot>>();
            services.AddTransient<IReadyProcessor<Bot.Core.Models.GateKeeperBot>, GateKeeperReadyProcessor>();
            
            services.AddTransient<IBusyProcessor, BusyProcessor>();
            services.AddTransient<IRightChecker, GateKeeper.Service.GKRightChecker>();
            services.AddTransient<IUserChecker, UserChecker>();

            services.AddSingleton<AsyncTaskExecutor>();
            services.AddTransient<Bot.Core.Services.Bot>();

            services.AddTransient<IDataBaseSettings, DBSettings>();
            services.AddSingleton<ConnectionsFactory>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
