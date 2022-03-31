using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Palantir.FullTextSearch.Models
{
    public class SearchResult
    {
        public static IEnumerable<SearchResult> Empty = ImmutableList.Create<SearchResult>();

        public readonly TextToken Token ;
        public readonly float Rank;

        public SearchResult(TextToken token, float rank)
        {
            Token = token;
            Rank = rank;
        }

    }
}
