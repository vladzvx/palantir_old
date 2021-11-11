using Common;
using Common.Interfaces;
using Common.Services;
using Common.Services.DataBase;
using Common.Services.DataBase.Interfaces;
using Common.Services.DataBase.Reading;
using Common.Services.gRPC.Subscribtions;
using Common.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationProvider.Services;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationProvider
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<CancellationTokenSource>();
            services.AddTransient<IWriterCore<Message>, MessagesWriterCore>();
            services.AddTransient<IWriterCore<Entity>, EntityWriterCore>();
            services.AddSingleton<ICommonWriter<Message>, CommonWriter<Message>>();
            services.AddSingleton<ICommonWriter<Entity>, CommonWriter<Entity>>();
            services.AddTransient<IDataBaseSettings, NotificationProvider.Services.DataBaseSetting>();
            services.AddSingleton<ConnectionsFactory>();
            services.AddHostedService<NotificationReciever>();
            services.AddHostedService<GrpcDataReciever>();
            services.AddTransient<ChatInfoLoader>();
            services.AddTransient<IGrpcSettings, GrpcSettings>();

            services.AddSingleton<ConnectionFactory>();
            services.AddTransient<IRabbitMQSettings, RabbitMQSettings>();
            services.AddSingleton<RabbitMQBasePublisher>();
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
            });
        }
    }
}
