namespace Minimalist.Reactive.Linq;

internal sealed class AllOperator<T> : IObservable<bool>
{
    private readonly IObservable<T> _source;
    private readonly Func<T, bool> _predicate;

    public AllOperator(IObservable<T> source, Func<T, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
    }

    public IDisposable Subscribe(IObserver<bool> observer)
    {
        var x = new All(observer, _predicate);
        return _source.Subscribe(x);
    }

    internal sealed class All : IObserver<T>
    {
        private readonly IObserver<bool> _observer;
        private readonly Func<T, bool> _predicate;

        public All(IObserver<bool> observer, Func<T, bool> predicate)
        {
            _observer = observer;
            _predicate = predicate;
        }

        public void OnNext(T value)
        {
            bool result;
            try
            {
                result = _predicate(value);
            }
            catch (Exception ex)
            {
                _observer.OnError(ex);
                return;
            }

            if (!result)
            {
                _observer.OnNext(false);
                _observer.OnCompleted();
            }
        }

        public void OnCompleted()
        {
            _observer.OnNext(true);
            _observer.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _observer.OnError(error);
        }
    }
}
