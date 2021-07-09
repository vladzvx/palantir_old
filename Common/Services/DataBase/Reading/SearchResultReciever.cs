﻿using Common.Services.DataBase.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services.DataBase.Reading
{
    public class StreamSearchResiever : ISearchResultReciever
    {
        private readonly ConcurrentQueue<SearchResult> results = new ConcurrentQueue<SearchResult>();
        private readonly object locker = new object();
        private bool complited = false;
        public bool IsComplited
        {
            get
            {
                lock (locker)
                {
                    return complited;
                }
            }
            set
            {
                lock (locker)
                {
                    complited = value;
                }
            }
        }

        public SearchResult[] ViewResults()
        {
            return results.ToArray();
        }

        public void Recieve(SearchResult searchResult)
        {
            results.Enqueue(searchResult);
        }

        public bool TryDequeueResult(out SearchResult searchResult)
        {
            return results.TryDequeue(out searchResult);
        }
    }
}