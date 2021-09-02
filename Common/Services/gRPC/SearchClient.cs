using Common.Services.DataBase.Interfaces;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.gRPC
{
    public class SearchClient
    {
        public readonly ISearchResultReciever searchResultReciever;
        private readonly GrpcChannel grpcChannel;
        public SearchClient(ISearchResultReciever searchResultReciever, GrpcChannel grpcChannel)
        {
            this.searchResultReciever = searchResultReciever;
            this.grpcChannel = grpcChannel;
        }
        public async Task Search(SearchRequest request, CancellationToken cancellationToken)
        {
            SearchProto.SearchProtoClient Client = new SearchProto.SearchProtoClient(grpcChannel);
            var searchResponse = Client.Search(request);
            while (await searchResponse.ResponseStream.MoveNext() && !cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Recieved!");
                searchResultReciever.Recieve(searchResponse.ResponseStream.Current);
            }
            searchResultReciever.IsComplited = true;
            Console.WriteLine("Data recieved!");
        }
    }
}
