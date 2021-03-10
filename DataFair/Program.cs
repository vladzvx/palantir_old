using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataFair
{
    public class Program
    {
        private static Regex reg = new Regex(@"^http://.+:(\d+)");
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var cfg = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                    var url = cfg.GetSection("Kestrel:EndpointDefaults:Url");
                    if (url.Exists())
                    {
                        //Math math = reg.Match()



                        webBuilder.ConfigureKestrel(serverOptions =>
                        {
                            serverOptions.Listen(IPAddress.Any, 5005, o => {
                                o.Protocols = HttpProtocols.Http2;
                            });
                        });
                    }

                    webBuilder.UseStartup<Startup>();
                });
    }
}
