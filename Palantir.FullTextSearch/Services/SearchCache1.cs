using Palantir.FullTextSearch.Interfaces;
using Palantir.FullTextSearch.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Palantir.FullTextSearch.Services
{
    public class SearchCache1 : ISearchCache
    {
        private readonly object mainIndexLocker = new object();
        private readonly object tempIndexLocker = new object();
        private readonly ITextPreparator textPreparator;
        private ImmutableDictionary<string, ImmutableList<Chunk>> MainIndex;
        private Dictionary<string, Dictionary<DateTime,Chunk.Stored>> tempIndex;
        private readonly Thread SavingThread;
        public SearchCache1(ITextPreparator textPreparator)
        {
            this.textPreparator = textPreparator;
        }
        
        public void SaveChunks()
        {
            Dictionary<string, Dictionary<DateTime, Chunk.Stored>> buffer;
            lock (tempIndexLocker)
            {
                buffer = tempIndex;
                tempIndex = new Dictionary<string, Dictionary<DateTime, Chunk.Stored>>();
            }
            var mainDictBuilder = MainIndex.ToBuilder();
            foreach (string key in buffer.Keys)
            {
                IEnumerable<Chunk> cunks = buffer[key].Values.Select(item => item.ToImmutable());
                if (!mainDictBuilder.ContainsKey(key))
                {
                    mainDictBuilder.Add(key,ImmutableList.CreateRange(cunks));
                }
                else
                {
                    mainDictBuilder[key] = mainDictBuilder[key].AddRange(cunks);
                }
            }
            var NewIndex = mainDictBuilder.ToImmutable();
            lock (mainIndexLocker)
                MainIndex = NewIndex;
        }

        public void Add(string text, Guid textId, DateTime? timestamp = null)
        {
            var words = textPreparator.Preparate(text, textId, timestamp);
            foreach (var word in words)
            {
                AddSingleToken(word.Word, word.Token);
            }
        }

        public void AddSingleToken(string word, TextToken.Stored token, DateTime? textTimestamp = default)
        {
            DateTime timeKey = textTimestamp ?? DateTime.MinValue;
            lock (tempIndexLocker)
            {
                if (!tempIndex.ContainsKey(word))
                {
                    tempIndex.Add(word, new Dictionary<DateTime, Chunk.Stored>());
                }
                var tmpDict = tempIndex[word];
                if (!tmpDict.ContainsKey(timeKey))
                {
                    tmpDict.Add(timeKey, new Chunk.Stored(word, timeKey));
                }
                tmpDict[timeKey].AddToken(token);
            }
        }

        public IEnumerable<SearchResult> Search(SearchRequest searchRequest)
        {
            if (MainIndex.TryGetValue(searchRequest.Word, out var texts))
            {
                Func<Chunk, bool> predicate;
                if (searchRequest.From != DateTime.MinValue && searchRequest.To != DateTime.MinValue)
                {
                    predicate = item => item.ChunkDate >= searchRequest.From && item.ChunkDate < searchRequest.To;
                }
                else if (searchRequest.From == DateTime.MinValue && searchRequest.To != DateTime.MinValue)
                {
                    predicate = item => item.ChunkDate < searchRequest.To;
                }
                else if (searchRequest.From != DateTime.MinValue && searchRequest.To == DateTime.MinValue)
                {
                    predicate = item => item.ChunkDate >= searchRequest.From;
                }
                else
                {
                    predicate = item => true;
                }
                return texts.Where(predicate).SelectMany(item => item.Tokens).Select(item => new SearchResult(item, 0)).OrderByDescending(item => item.Rank).Take(searchRequest.Limit);
            }
            else return SearchResult.Empty;
        }
    }
}
