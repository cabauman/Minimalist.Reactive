using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimalist.Reactive.Concurrency
{
    public class VirtualTimeScheduler : IScheduler
    {
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
