namespace Minimalist.Reactive.Linq;

internal sealed class WhereOperator<T> : IObservable<T>
{
    private readonly IObservable<T> _source;
    private readonly Func<T, bool> _predicate;

    public WhereOperator(IObservable<T> source, Func<T, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        var x = new Where(observer, _predicate);
        var subscription = _source.Subscribe(x);
        x.SetSubscription(subscription);
        return subscription;
    }

    internal sealed class Where : IObserver<T>, IDisposable
    {
        private IObserver<T> _observer;
        private readonly Func<T, bool> _predicate;
        private IDisposable? _subscription;

        public Where(IObserver<T> observer, Func<T, bool> predicate)
        {
            _observer = observer;
            _predicate = predicate;
        }

        public void OnNext(T value)
        {
            try
            {
                if (_predicate(value))
                {
                    _observer.OnNext(value);
                }
            }
            catch (Exception exception)
            {
                _observer.OnError(exception);
                Dispose();
            }

        }

        public void OnCompleted()
        {
            _observer.OnCompleted();
            Dispose();
        }

        public void OnError(Exception error)
        {
            _observer.OnError(error);
            Dispose();
        }

        public void SetSubscription(IDisposable subscription)
        {
            _subscription = subscription;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _observer, NopObserver<T>.Instance) != NopObserver<T>.Instance)
            {
                _subscription?.Dispose();
            }
        }
    }
}
