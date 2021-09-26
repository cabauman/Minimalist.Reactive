namespace Minimalist.Reactive.Linq.ObservableImpl
{
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
            return _source.Subscribe(x);
        }

        internal sealed class Where : IObserver<T>
        {
            private readonly IObserver<T> _observer;
            private readonly Func<T, bool> _predicate;

            public Where(IObserver<T> observer, Func<T, bool> predicate)
            {
                _observer = observer;
                _predicate = predicate;
            }

            public void OnNext(T value)
            {
                if (_predicate(value))
                {
                    _observer.OnNext(value);
                }
            }

            public void OnCompleted()
            {
                _observer.OnCompleted();
                // Dispose upstream and set observer to a NoOp observer.
            }

            public void OnError(Exception error)
            {
                _observer.OnError(error);
                // Dispose upstream and set observer to a NoOp observer.
            }
        }
    }
}