using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Common.Services
{
    public abstract class ActionPeriodicExecutor
    {
        private readonly Timer timer;
        private Task workingTask;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly object locker = new object();

        internal Action<object?> ExecutingAction { get; private set; }

        public ActionPeriodicExecutor(double RepeatingInterval, CancellationTokenSource cancellationTokenSource, Action<object?> action =null)
        {
            this.ExecutingAction = action;
            this.cancellationTokenSource = cancellationTokenSource;
            timer = new Timer();
            timer.Interval = RepeatingInterval;
            timer.Elapsed += TimerAction;
            timer.AutoReset = true;
        }

        public void SetAction(Action<object?> action)
        {
            if (Monitor.TryEnter(locker))
            {
                ExecutingAction = action;
                Monitor.Exit(locker);
            }
        }
        public void Start()
        {
            timer.Start();
        }
        private void TimerAction(object sender, ElapsedEventArgs e)
        {
            if (Monitor.TryEnter(locker))
            {
                if (ExecutingAction != null&&(workingTask == null || workingTask.IsCompleted))
                    workingTask = Task.Factory.StartNew(ExecutingAction, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
                Monitor.Exit(locker);
            }
        }
    }
}
