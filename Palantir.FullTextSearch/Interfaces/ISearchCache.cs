using Palantir.FullTextSearch.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Palantir.FullTextSearch.Interfaces
{
    public interface ISearchCache
    {
        public void Add(string text, Guid textId, DateTime? timestamp = null);
        //public IEnumerable<SearchResult> Search(SearchRequest searchRequest);
    }
}
