using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services
{
    public class AsuncExecutor
    {
        private List<Task> tasks;
        public async Task Action(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                tasks.RemoveAll(task => task.IsCompleted);
                tasks.Add(Task.Delay(100));
                await Task.WhenAny(tasks);
            }
        }
    }
}
