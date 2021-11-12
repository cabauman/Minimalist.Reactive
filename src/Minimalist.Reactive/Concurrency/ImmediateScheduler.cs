using System;
using System.Threading;

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
            return action(this, state);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var dt = Scheduler.Normalize(dueTime);
            if (dt.Ticks > 0)
            {
                Thread.Sleep(dt);
            }

            return action(this, state);
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var due = Scheduler.Normalize(dueTime - Now);
            return Schedule(state, TimeSpan.Zero, action);
        }
    }
}
