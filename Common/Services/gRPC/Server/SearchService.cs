using Common.Services.DataBase;
using Grpc.Core;
using System.Linq;
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
            SearchingTask = await searchProvider.AsyncSearch(request.SearchType, request.Request, request.StartTime.ToDateTime(), request.EndTime.ToDateTime(), request.Limit == 0 ? 1500 : request.Limit, request.IsChannel, request.IsGroup, tokenSource.Token, request.Ids != null && request.Ids.Count > 0 ? request.Ids.ToArray() : null);
            bool lastExecutionEnable = true;
            while (!SearchingTask.IsCompleted || lastExecutionEnable)
            {
                while (searchProvider.searchResultReciever.TryDequeueResult(out SearchResult res))
                {
                    await responseStream.WriteAsync(res);
                }
                await Task.Delay(50);
                if (SearchingTask.IsCompleted && lastExecutionEnable)
                {
                    lastExecutionEnable = false;
                }
            }
        }
    }
}
