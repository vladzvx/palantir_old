using Common;
using Common.Services.DataBase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchLoader.Services
{
    public class SearchResultsReciever : ISearchResultReciever
    {
        public bool IsComplited { get ; set ; }
        public DateTime? FirstRecieved = null;
        public int count = 0;
        public void Recieve(SearchResult searchResult)
        {
            if (FirstRecieved == null) FirstRecieved = DateTime.UtcNow;
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
