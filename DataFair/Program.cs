using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("DataFairTests")]

namespace DataFair
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //if (Storage.Chats.Count!=0)
            //{
            //    int g = 0;
            //}
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var cfg = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                    var url = cfg.GetSection("Kestrel:EndpointDefaults:Url");

                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Listen(IPAddress.Any, 5005, o => {
                            o.Protocols = HttpProtocols.Http2;
                           // o.UseHttps();
                        });
                        //serverOptions.ConfigureHttpsDefaults(https=> 
                        //{
                        //    https.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                        //        "mysert.pfx","pwwwwd");
                        //});
                        //serverOptions.Listen(IPAddress.Any, 5004, o =>
                        //{
                        //    o.Protocols = HttpProtocols.Http1;
                        //});
                    });



                    webBuilder.UseStartup<Startup>();
                });
    }
}
