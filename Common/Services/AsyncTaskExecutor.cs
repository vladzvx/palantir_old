using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services
{
    public class AsyncTaskExecutor : IDisposable
    {
        private readonly Task Executor;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        public AsyncTaskExecutor()
        {
            Executor = Task.Factory.StartNew((token) =>
             {
                 List<Task> buffer = new List<Task>();
                 if (token is CancellationToken ct)
                 {
                     while (!ct.IsCancellationRequested)
                     {
                         int i = 0;
                         buffer.RemoveAll(item => item.IsCompleted);
                         while (i<150 && queue.TryDequeue(out Task task))
                         {
                             buffer.Add(task);
                             i++;
                         }
                         Task t = Task.Delay(100);
                         buffer.Add(t);
                         Task.WaitAll(t, Task.WhenAny(buffer));
                     }
                 }

             }, cts.Token, TaskCreationOptions.LongRunning);
        }
        private readonly ConcurrentQueue<Task> queue = new ConcurrentQueue<Task>();
        public void Add(Task task)
        {
            queue.Enqueue(task);
        }

        public void Dispose()
        {
            cts.Cancel();
        }
    }
}
