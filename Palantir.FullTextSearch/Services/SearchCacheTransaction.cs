using Palantir.FullTextSearch.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Palantir.FullTextSearch.Services
{
    public class SearchCacheTransaction : ISearchCacheTransaction
    {
        public Guid TransactionId { get; init; }
        public CancellationToken CommitToken { get; init; }
        public CancellationToken RollbackToken { get; init; }
        private readonly CancellationTokenSource CommitCancellationTokenSource;
        private readonly CancellationTokenSource RollbackCancellationTokenSource;
        public SearchCacheTransaction()
        {
            CommitCancellationTokenSource = new CancellationTokenSource();
            RollbackCancellationTokenSource = new CancellationTokenSource();
        }
        public Task Commit()
        {
            throw new NotImplementedException();
        }

        public Task Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
