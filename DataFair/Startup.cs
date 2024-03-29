﻿using Common;
using Common.Services;
using Common.Services.DataBase;
using Common.Services.DataBase.DataProcessing;
using Common.Services.DataBase.Interfaces;
using Common.Services.DataBase.Reading;
using Common.Services.gRPC;
using Common.Services.gRPC.Subscribtions;
using Common.Services.Interfaces;
using Common.Services.Managers;
using DataFair.Services;
using DataFair.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace DataFair
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<CancellationTokenSource>();
            services.AddSingleton<State>();

            services.AddSingleton<ICommonWriter, CommonWriter>();
            services.AddSingleton<ILoadManager, LoadManager>();
            services.AddSingleton<ISettings, Settings>();
            services.AddSingleton<ILimits, Limits>();
            services.AddSingleton<ConnectionsFactory>();

            services.AddSingleton<ICommonWriter<Entity>, CommonWriter<Entity>>();
            services.AddSingleton<ICommonWriter<Message>, CommonWriter<Message>>();


            services.AddTransient<DataPreparator>();
            services.AddTransient<IDataBaseSettings, DataBaseSettings>();
            services.AddTransient<IWriterCore<Message>, MessagesWriterCore>();
            services.AddTransient<IWriterCore<Entity>, EntityWriterCore>();
            services.AddTransient<IWriterCore, WriterCore>();
            services.AddTransient<ISearchResultReciever, StreamSearchResiever>();
            services.AddTransient<ICommonReader<ChatInfo>, ChatInfoReader>();
            services.AddTransient<ChatInfoLoader>();

            services.AddTransient<StateReport>();
            //services.AddTransient<SystemReport>();
            services.AddTransient<IOrdersGenerator, OrdersGenerator>();
            services.AddTransient<SearchProvider>();


            services.AddHostedService<OrdersManager>();
            services.AddHostedService<CollectorsManager>();
            //services.AddHostedService<MediaAndFormattingProcessor>();
            services.AddHostedService<TextVectorizer>();
            //services.AddHostedService<StoredProcedureExecutor>();
            services.AddGrpc();
            services.AddCors();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<OrderBoardService>();
                endpoints.MapGrpcService<ConfiguratorService>();
                endpoints.MapGrpcService<SubscribtionService>();
                endpoints.MapGrpcService<SearchService>();

                endpoints.MapControllers();
            });
        }
    }
}
