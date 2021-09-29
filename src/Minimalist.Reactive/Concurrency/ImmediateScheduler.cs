using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimalist.Reactive.Concurrency
{
    public sealed class ImmediateScheduler : IScheduler
    {
        private static readonly Lazy<ImmediateScheduler> StaticInstance = new(static () => new ImmediateScheduler());

        private ImmediateScheduler()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the immediate scheduler.
        /// </summary>
        public static ImmediateScheduler Instance => StaticInstance.Value;

        public DateTimeOffset Now => DateTimeOffset.UtcNow;

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotImplementedException();
            //return action(state);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var dt = Scheduler.Normalize(dueTime);
            if (dt.Ticks > 0)
            {
                Thread.Sleep(dt);
            }

            throw new NotImplementedException();
            //return action(state);
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotImplementedException();
        }
    }
}
