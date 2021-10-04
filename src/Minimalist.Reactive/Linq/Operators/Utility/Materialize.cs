namespace Minimalist.Reactive.Linq
{
    internal sealed class MaterializeOperator<TSource> : IObservable<Notification<TSource>>
    {
        private readonly IObservable<TSource> _source;

        public MaterializeOperator(IObservable<TSource> source)
        {
            _source = source;
        }

        public IDisposable Subscribe(IObserver<Notification<TSource>> observer)
        {
            var x = new Materialize(observer);
            return _source.Subscribe(x);
        }

        internal sealed class Materialize : IObserver<TSource>
        {
            private readonly IObserver<Notification<TSource>> _observer;

            public Materialize(IObserver<Notification<TSource>> observer)
            {
                _observer = observer;
            }

            public void OnNext(TSource value)
            {
                _observer.OnNext(Notification.CreateOnNext(value));
            }

            public void OnError(Exception error)
            {
                _observer.OnNext(Notification.CreateOnError<TSource>(error));
                OnCompleted();
            }

            public void OnCompleted()
            {
                _observer.OnNext(Notification.CreateOnCompleted<TSource>());
                _observer.OnCompleted();
            }
        }
    }
}
