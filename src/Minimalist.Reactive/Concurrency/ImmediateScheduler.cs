using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimalist.Reactive.Concurrency
{
    public class ImmediateScheduler : IScheduler
    {
        private static readonly Lazy<ImmediateScheduler> StaticInstance = new(static () => new ImmediateScheduler());

        private ImmediateScheduler()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the immediate scheduler.
        /// </summary>
        public static ImmediateScheduler Instance => StaticInstance.Value;

        public DateTimeOffset Now => throw new NotImplementedException();

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotImplementedException();
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotImplementedException();
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotImplementedException();
        }
    }
}
