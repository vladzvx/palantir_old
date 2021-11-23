using Common.Services.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public class MongoWriter
    {
        private readonly Channel<Update> channel = Channel.CreateUnbounded<Update>();
        private readonly MongoClient mongoClient;
        private Thread worker;
        public long BotId { get; private set; }
        public MongoWriter(MongoClient mongoClient)
        {
            this.mongoClient = mongoClient;
            worker = new Thread(Worker);
        }

        public void Start(long BotId)
        {
            this.BotId = BotId;
            worker.Start();
        }

        private void Worker()
        {
            while (true)
            {
                try
                {
                    IMongoDatabase db = mongoClient.GetDatabase("messages", new MongoDatabaseSettings());
                    IMongoCollection<Message> collection = db.GetCollection<Message>(BotId.ToString());
                    List<Message> messages = new List<Message>();
                    while (channel.Reader.WaitToReadAsync().AsTask().Result)
                    {
                        int count = 0;
                        while (channel.Reader.TryRead(out Update update) && count < 1000)
                        {
                            count++;
                            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                            {
                                messages.Add(update.Message);
                            }
                        }
                        if (messages.Count > 0)
                        {
                            collection.InsertMany(messages);
                            messages.Clear();
                        }
                    }
                }
                catch (Exception ex) { }
                Thread.Sleep(10000);
            }

        }

        public async Task PutData(Update data)
        {
            await channel.Writer.WriteAsync(data);
        }
    }
}
