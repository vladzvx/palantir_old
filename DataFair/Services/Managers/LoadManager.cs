using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataFair.Services
{
    public class LoadManager
    {
        private bool slowMode = false;
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void AddValue(int size)
        {
            if (size > Options.SleepModeStartCount)
            {
                if (cacheLock.TryEnterWriteLock(-1))
                {
                    slowMode = true;
                    cacheLock.ExitWriteLock();
                }
            }
            else if (size <= Options.SleepModeStartCount / 2)
            {
                if (cacheLock.TryEnterWriteLock(-1))
                {
                    slowMode = false;
                    cacheLock.ExitWriteLock();
                }
            }  
        }

        public async Task WaitIfNeed()
        {
            if (cacheLock.TryEnterReadLock(Options.Pasue))
            {
                if (!slowMode)
                {
                    return;
                }
                else
                {
                    await Task.Delay(Options.Pasue);
                }
                cacheLock.ExitReadLock();
            }
            
        }
    }
}
