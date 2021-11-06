using Bot.Core.Interfaces;
using Bot.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Core.Services
{
    public class DataStorage<TBot> : IDataStorage<TBot> where TBot: IConfig, new()
    {
        private readonly MongoClient mongoClient;
        public DataStorage(MongoClient mongoClient)
        {
            this.mongoClient = mongoClient;
        }
        public async Task<TBot> GetChat(long id, CancellationToken token, long botId)
        {
            IMongoDatabase db = mongoClient.GetDatabase("bots", new MongoDatabaseSettings());
            IMongoCollection<TBot> collection = db.GetCollection<TBot>(botId.ToString());
            var filt  = Builders<TBot>.Filter.Eq("_id", id);
            var res = await collection.FindAsync(filt, cancellationToken: token);
            return await res.FirstOrDefaultAsync(token);
        }

        public async Task<Page> GetPage(ObjectId guid, CancellationToken token, long botId)
        {
            IMongoDatabase db = mongoClient.GetDatabase("bots", new MongoDatabaseSettings());
            IMongoCollection<Page> collection = db.GetCollection<Page>(typeof(Page).Name + "s");
            var filt = Builders<Page>.Filter.Eq("_id", guid);
            var res = await collection.FindAsync(filt, cancellationToken: token);
            return await res.FirstOrDefaultAsync(token);
        }

        public async Task SaveChat(TBot bot, CancellationToken token, long botId)
        {
            IMongoDatabase db = mongoClient.GetDatabase("bots", new MongoDatabaseSettings());
            IMongoCollection<TBot> collection = db.GetCollection<TBot>(botId.ToString());
            await collection.ReplaceOneAsync(Builders<TBot>.Filter.Eq("_id", bot.Id), bot, cancellationToken: token, options:new ReplaceOptions() {IsUpsert=true });
        }

        public async Task SaveBot(CancellationToken token, long botId, string username)
        {
            IMongoDatabase db = mongoClient.GetDatabase("bots", new MongoDatabaseSettings());
            IMongoCollection<BotInfo> collection = db.GetCollection<BotInfo>(botId.ToString());
            await collection.ReplaceOneAsync(Builders<BotInfo>.Filter.Eq("_id", botId), new BotInfo() {_id= botId,Username=username }, cancellationToken: token, options: new ReplaceOptions() { IsUpsert = true });
        }


        public async Task SavePages(IEnumerable<Page> pages, CancellationToken token, long botId)
        {
            IMongoDatabase db = mongoClient.GetDatabase("bots", new MongoDatabaseSettings());
            IMongoCollection<Page> collection = db.GetCollection<Page>(typeof(Page).Name + "s");
            await collection.InsertManyAsync( pages, cancellationToken: token);
        }

        //public async Task SavePage(Page page, CancellationToken token)
        //{
        //    IMongoDatabase db = mongoClient.GetDatabase("bots", new MongoDatabaseSettings());
        //    IMongoCollection<Page> collection = db.GetCollection<Page>(typeof(Page).Name + "s");
        //    await collection.InsertOneAsync(page, cancellationToken: token);
        //}
    }
}
