using Bot.Core.Models;
using Common;
using Common.Services.gRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Core.Services
{
    public class SearchReciever
    {


        private readonly SearchClient searchClient;
        private readonly SearchState searchState;


        public SearchReciever(SearchClient searchClient, SearchState searchState)
        {
            this.searchClient = searchClient;
            this.searchState = searchState;
        }

        public async Task Search(long user,SearchRequest searchRequest, CancellationToken token)
        {
            Guid guid = Guid.NewGuid();
            Task searchTask = searchClient.Search(searchRequest, token);
            Task processingTask = Task.Factory.StartNew(async () => 
            {
                bool lastExecutionEnable = true;
                int number = 0;
                Page currentPage = new Page(guid, number);
                while (!searchTask.IsCompleted || lastExecutionEnable)
                {
                    while (searchClient.searchResultReciever.TryDequeueResult(out var result))
                    {
                        if (!currentPage.TryAddResult(result))
                        {
                            await searchState.SavePage(user,currentPage, token);
                            number++;
                            currentPage = new Page(guid, number);
                        }
                    }

                    if (searchTask.IsCompleted && lastExecutionEnable)
                        lastExecutionEnable = false;
                }
            },TaskCreationOptions.LongRunning);
            await Task.WhenAll(searchTask,processingTask);
        }
    }
}
