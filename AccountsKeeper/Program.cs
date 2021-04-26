using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeleSharp.TL.Updates;
using TLSharp.Core;
using TLSharp.Core.Exceptions;

namespace AccountsKeeper
{
    public class ClientWrapper
    {
        public async Task test()
        {
            string phone = "";
            string password = "";
            string ApiHash = "";;;
            string SessionName = "";;;
            int ApiId = 0;


            var client = new TelegramClient(ApiId, ApiHash, sessionUserId: SessionName);
            await client.ConnectAsync();
            if (!client.IsConnected)
            {
                var hash = await client.SendCodeRequestAsync(phone);
                var code = Console.ReadLine();

                var user = await client.MakeAuthAsync("phone", hash, code);


                await client.SendCodeRequestAsync(phone);
                try
                {
                    await client.MakeAuthAsync(phone, hash, code);
                }
                catch (CloudPasswordNeededException)
                {
                    try
                    {

                    }
                    catch (InvalidCastException)
                    {
                        var passwordSettings = await client.GetPasswordSetting();
                        await client.MakeAuthWithPasswordAsync(passwordSettings, password);
                        if (!client.IsConnected) return;
                    }
                }
                while (true)
                {
                    var state = await client.SendRequestAsync<TLState>(new TLRequestGetState());
                    var req = new TLRequestGetDifference() { Date = state.Date, Pts = state.Pts, Qts = state.Qts };
                    var adiff = await client.SendRequestAsync<TLAbsDifference>(req);
                    if (!(adiff is TLDifferenceEmpty))
                    {
                        if (adiff is TLDifference)
                        {
                            var diff = adiff as TLDifference;
                            Console.WriteLine("new:" + diff.NewMessages.Count);
                        }
                        else if (adiff is TLDifferenceTooLong)
                            Console.WriteLine("too long");
                        else if (adiff is TLDifferenceSlice)
                            Console.WriteLine("slice");
                    }
                    await Task.Delay(500);
                }


            }

        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {

            ClientWrapper clientWrapper = new ClientWrapper();

            clientWrapper.test().Wait();

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
