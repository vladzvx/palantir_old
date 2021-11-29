using Bot.Core;
using Bot.Core.Interfaces;
using Bot.Core.Interfaces.BotFSM;
using Bot.Core.Models;
using Bot.Core.Services;
using Bot.Service.Services;
using Common;
using Common.Services;
using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Common.Services.DataBase.Reading;
using Common.Services.gRPC;
using Common.Services.Interfaces;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Threading;
using Telegram.Bot.Types;

namespace Bot.Service
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ConventionRegistry.Register("IgnoreIfNullConvention", new ConventionPack { new IgnoreIfNullConvention(true) }, t => true);
            services.AddControllers();
            services.AddSingleton(new CancellationTokenSource());
            services.AddSingleton<IBotSettings, BotSettings>();
            IGrpcSettings grpcSettings = new GrpcSettings();
            services.AddSingleton(GrpcChannel.ForAddress(grpcSettings.Url));


            services.AddHostedService<BotsEntryPoint<SearchBot>>();
            services.AddSingleton<ISenderSettings, SenderSettings>();
            services.AddSingleton<IMessagesSender, MessagesSender>();
            services.AddSingleton<MongoWriter>();
            services.AddSingleton<IDataStorage<SearchBot>, DataStorage<SearchBot>>();
            services.AddSingleton(new MongoClient(Options.MongoConnectionString));

            services.AddTransient<SearchClient>();
            services.AddTransient<SearchReciever>();

            services.AddTransient<IFSMFactory<SearchBot>, SubFSMFactory>();
            services.AddTransient<IReadyProcessor<SearchBot>, SearchReadyProcessor>();
            services.AddTransient<IBusyProcessor, BusyProcessor>();
            services.AddTransient<IRightChecker, PrivateRightChecker>();

            services.AddTransient<SearchState>();
            services.AddSingleton<AsyncTaskExecutor>();
            services.AddTransient<ISearchResultReciever, StreamSearchResiever>();
            services.AddTransient<Bot.Core.Services.Bot>();
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
