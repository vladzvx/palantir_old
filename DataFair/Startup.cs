using Common;
using DataFair.Services;
using DataFair.Services.Interfaces;
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

            services.AddSingleton<ICommonWriter<Message>,CommonWriter<Message>>();
            services.AddSingleton<ICommonWriter<Chat>, CommonWriter<Chat>>();
            services.AddSingleton<ICommonWriter<User>, CommonWriter<User>>();

            services.AddTransient<IWriterCore<Message>,MessageWriterCore>();
            services.AddTransient<IWriterCore<User>, UserWriterCore>();
            services.AddTransient<IWriterCore<Chat>, ChatWriterCore>();
            services.AddTransient<StateReport>();
            services.AddTransient<SystemReport>();
            services.AddTransient<OrdersGenerator>();
            services.AddTransient<LoadManager>();

            services.AddHostedService<OrdersManager>();
            services.AddHostedService<CollectorsManager>();

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
