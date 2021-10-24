using Common;
using Common.Services.DataBase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SearchLoader.Services
{
    public class SearchResultsTestReciever : ISearchResultReciever
    {
        public Task Searching;
        //private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private bool _IsComplited = false;
        public bool IsComplited { get 
            {
                return _IsComplited;
            }
            set 
            {
                if (FirstRecieved == null)
                {
                    FirstRecieved = DateTime.UtcNow;
                }
                _IsComplited = true;
            } 
        }
        public DateTime? FirstRecieved = null;
        public DateTime CreationTime;
        public int count = 0;

        public SearchResultsTestReciever()
        {
            CreationTime = DateTime.UtcNow;
            //Searching = Task.Delay(-1, cts.Token);
        }
        public void Recieve(SearchResult searchResult)
        {
            if (FirstRecieved == null)
            {
                FirstRecieved = DateTime.UtcNow;
                //cts.Cancel();
            }
            count++;
        }

        public bool TryDequeueResult(out SearchResult searchResult)
        {
            searchResult = null;
            return false;
        }

        public SearchResult[] ViewResults()
        {
            return new SearchResult[1];
        }
    }
}
