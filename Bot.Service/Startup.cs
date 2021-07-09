using Bot.Core.Interfaces;
using Bot.Core.Services;
using Bot.Service.Services;
using Common.Services.DataBase.Interfaces;
using Common.Services.DataBase.Reading;
using Common.Services.gRPC;
using Common.Services.Interfaces;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Service
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllers();

            services.AddSingleton(new CancellationTokenSource());
            services.AddSingleton<IBotSettings, BotSettings>();
            IGrpcSettings grpcSettings = new GrpcSettings();
            services.AddSingleton(GrpcChannel.ForAddress(grpcSettings.Url));
            services.AddHostedService<BotsEntryPoint>();

            services.AddTransient<BotMessageHandler>();
            services.AddTransient<SearchClient>();
            services.AddTransient<ISearchResultReciever, StreamSearchResiever>();
            services.AddTransient<Bot.Core.Services.Bot>();
            AppContext.SetSwitch(
    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
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
