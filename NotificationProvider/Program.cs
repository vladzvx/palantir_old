using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NotificationProvider
{
    public class Program
    {
        public static void Main(string[] args)
        {

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            List<Task> tsks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tsks.Add(Task.Factory.StartNew(() =>
                {
                    for (int i2 = 0; i2 < 10; i2++)
                    {
                        byte[] rndm = new byte[1];
                        rng.GetBytes(rndm);
                        Console.WriteLine(rndm[0]);

                    }
                }));
            }
            Task.WhenAll(tsks).Wait();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
