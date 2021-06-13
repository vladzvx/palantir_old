using Common;
using Common.Services;
using Common.Services.DataBase.DataProcessing;
using Common.Services.gRPC;
using Common.Services.Interfaces;
using DataFair.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddSingleton<State>();

            services.AddSingleton<ICommonWriter,CommonWriter>();
            services.AddSingleton<LoadManager>();

            services.AddTransient<DataPreparator>();
            services.AddTransient<IWriterCore,WriterCore>();
            services.AddTransient<StateReport>();
            services.AddTransient<SystemReport>();
            services.AddTransient<OrdersGenerator>();


            services.AddHostedService<OrdersManager>();
            services.AddHostedService<CollectorsManager>();
            services.AddHostedService<MediaAndFormattingProcessor>();

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

                endpoints.MapControllers();
            });
        }
    }
}
