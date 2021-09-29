namespace Minimalist.Reactive.Linq
{
    internal sealed class SelectOperator<T, TResult> : IObservable<TResult>
    {
        private readonly IObservable<T> _source;
        private readonly Func<T, TResult> _selector;

        public SelectOperator(IObservable<T> source, Func<T, TResult> selector)
        {
            _source = source;
            _selector = selector;
        }

        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            var x = new Select(observer, _selector);
            return _source.Subscribe(x);
        }

        internal sealed class Select : IObserver<T>
        {
            private readonly IObserver<TResult> _observer;
            private readonly Func<T, TResult> _selector;

            public Select(IObserver<TResult> observer, Func<T, TResult> selector)
            {
                _observer = observer;
                _selector = selector;
            }

            public void OnNext(T value)
            {
                _observer.OnNext(_selector(value));
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