using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public partial class Order
    {
        public enum Status
        {
            Created,
            Getted,
            Executed
        }
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        private Status stat;
        public Status status { 
            get
            {
                cacheLock.EnterReadLock();
                Status result = stat;
                cacheLock.ExitReadLock();
                return result;
            }
            set
            {
                cacheLock.EnterWriteLock();
                stat = value;
                cacheLock.ExitWriteLock();
            }
        }
    }
}
