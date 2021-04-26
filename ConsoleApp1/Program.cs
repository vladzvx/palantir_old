using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Channels;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Updates;
using TLSharp.Core;
using TLSharp.Core.Exceptions;

namespace ConsoleApp1
{
    public class ClientWrapper
    {


        public async Task test()
        {
            string phone = "1";
            string password = "";
            string ApiHash = "1";
            //string SessionName = "test"; 
            int ApiId = 1;


            var client = new TelegramClient(ApiId, ApiHash);
            await client.ConnectAsync();
            if (client.IsConnected)
            {
                if (!client.IsUserAuthorized())
                {
                    var hash = await client.SendCodeRequestAsync(phone);
                    var code = Console.ReadLine();
                    try
                    {
                        var user =  await client.MakeAuthAsync(phone, hash, code);
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
                }




                var channelInfo = (await client.SendRequestAsync<TeleSharp.TL.Contacts.TLResolvedPeer>(
                new TeleSharp.TL.Contacts.TLRequestResolveUsername
                {
                    Username = "ekvinokurova"
                }).ConfigureAwait(false)).Chats[0] as TeleSharp.TL.TLChannel;

                //TeleSharp.TL.Messages.TLChatFull channelInfo11 = await client.SendRequestAsync<TeleSharp.TL.Messages.TLChatFull>
                //        (new TLRequestGetFullChat() { ChatId = channelInfo.Id, });

                int q = 0;
                var t = await client.SendRequestAsync<TeleSharp.TL.Messages.TLChatFull>
                    (new TLRequestGetFullChannel() {Channel = new TLInputChannel() {ChannelId= channelInfo.Id, AccessHash= (long)channelInfo.AccessHash } });
                //var qq = t.Chats[0] 
                //var messages = await client.GetHistoryAsync(peer, offset, max_id, msgCount)

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
                    await Task.Delay(100);
                }


            }

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ClientWrapper clientWrapper = new ClientWrapper();

            clientWrapper.test().Wait();
        }
    }
}
