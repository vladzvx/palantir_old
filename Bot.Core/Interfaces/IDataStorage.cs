using Bot.Core.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Core.Interfaces
{
    public interface IDataStorage<TBot> where TBot:IConfig, new()
    {
        public Task<Page> GetPage(ObjectId guid, CancellationToken token);
        public Task SavePages(IEnumerable<Page> pages, CancellationToken token);
        //public Task SavePage(Page page, CancellationToken token);
        public Task<TBot> GetChat(long id, CancellationToken token);
        public Task SaveChat(TBot bot, CancellationToken token);
    }
}
