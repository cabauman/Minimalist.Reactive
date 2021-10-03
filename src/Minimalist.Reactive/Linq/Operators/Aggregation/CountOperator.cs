namespace Minimalist.Reactive.Linq
{
    internal sealed class CountOperator<T> : IObservable<int>
    {
        private readonly IObservable<T> _source;

        public CountOperator(IObservable<T> source)
        {
            _source = source;
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            var x = new Count(observer);
            return _source.Subscribe(x);
        }

        internal sealed class Count : IObserver<T>
        {
            private readonly IObserver<int> _observer;
            private int _count;

            public Count(IObserver<int> observer)
            {
                _observer = observer;
            }

            public void OnNext(T value)
            {
                try
                {
                    checked
                    {
                        _count += 1;
                    }
                }
                catch (Exception ex)
                {
                    _observer.OnError(ex);
                }
            }

            public void OnCompleted()
            {
                _observer.OnNext(_count);
                _observer.OnCompleted();
            }

            public void OnError(Exception error)
            {
                _observer.OnError(error);
            }
        }
    }

    internal sealed class PredicateCountOperator<T> : IObservable<int>
    {
        private readonly IObservable<T> _source;
        private readonly Func<T, bool> _predicate;

        public PredicateCountOperator(IObservable<T> source, Func<T, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            var x = new Count(observer, _predicate);
            return _source.Subscribe(x);
        }

        internal sealed class Count : IObserver<T>
        {
            private readonly IObserver<int> _observer;
            private readonly Func<T, bool> _predicate;
            private int _count;

            public Count(IObserver<int> observer, Func<T, bool> predicate)
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
                        checked
                        {
                            _count += 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _observer.OnError(ex);
                }
            }

            public void OnCompleted()
            {
                _observer.OnNext(_count);
                _observer.OnCompleted();
            }

            public void OnError(Exception error)
            {
                _observer.OnError(error);
            }
        }
    }
}