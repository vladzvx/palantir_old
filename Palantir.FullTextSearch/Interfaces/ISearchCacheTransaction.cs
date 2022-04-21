using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Palantir.FullTextSearch.Interfaces
{
    public interface ISearchCacheTransaction
    {
        public Guid TransactionId { get; }
        public CancellationToken CommitToken { get; }
        public CancellationToken RollbackToken { get; }
        public Task Commit();
        public Task Rollback();
    }
}
