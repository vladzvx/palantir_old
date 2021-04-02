using Common;
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
    internal static class Const
    {
        public static string cnnstr = Environment.GetEnvironmentVariable("ConnectionString");
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            bool r = Const.cnnstr != null;
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Listen(IPAddress.Any, 5005, o => {
                            o.Protocols = HttpProtocols.Http2;
                        });                     
                        serverOptions.Listen(IPAddress.Any, 5002, o =>
                        {
                            o.Protocols = HttpProtocols.Http1;
                        });
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
