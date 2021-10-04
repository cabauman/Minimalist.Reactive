namespace Minimalist.Reactive.Concurrency
{
    public sealed class CurrentThreadScheduler : IScheduler
    {
        private static readonly Lazy<CurrentThreadScheduler> StaticInstance = new(() => new CurrentThreadScheduler());

        private CurrentThreadScheduler()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the current thread scheduler.
        /// </summary>
        public static CurrentThreadScheduler Instance => StaticInstance.Value;

        public DateTimeOffset Now => DateTimeOffset.UtcNow;

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
