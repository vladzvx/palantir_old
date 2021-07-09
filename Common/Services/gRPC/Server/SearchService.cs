﻿using Common.Services.DataBase;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.gRPC
{
    public class SearchService : SearchProto.SearchProtoBase
    {
        private readonly SearchProvider searchProvider;
        private Task SearchingTask;
        private readonly CancellationTokenSource tokenSource;
        public SearchService(SearchProvider searchProvider)
        {
            this.searchProvider = searchProvider;
            tokenSource = new CancellationTokenSource();
        }
        public async override Task Search(SearchRequest request, IServerStreamWriter<SearchResult> responseStream, ServerCallContext context)
        {
            int count = 0;
            SearchingTask = Task.Factory.StartNew(() => 
            {
                searchProvider.CommonSearch(request.SearchType, request.Request, request.StartTime.ToDateTime(), request.EndTime.ToDateTime(), request.Limit, request.IsChannel, request.IsGroup, tokenSource.Token, request.Ids != null && request.Ids.Count > 0 ? request.Ids.ToArray() : null).Wait();
            },TaskCreationOptions.LongRunning);

            while (!SearchingTask.IsCompleted || !searchProvider.searchResultReciever.IsComplited)
            {
                while (searchProvider.searchResultReciever.TryDequeueResult(out SearchResult res))
                {
                    await responseStream.WriteAsync(res);
                    count++;
                    if (count > 100000)
                    {
                        tokenSource.Cancel();
                        break;
                    }
                }
                await Task.Delay(200);
                if (SearchingTask.IsCompleted && searchProvider.searchResultReciever.ViewResults().Length==0) searchProvider.searchResultReciever.IsComplited = true;
            }
        }
    }
}