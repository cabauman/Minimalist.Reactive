namespace Minimalist.Reactive
{
    internal class ReturnProducer<T> : IObservable<T>
    {
        private readonly T _value;

        public ReturnProducer(T value)
        {
            _value = value;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(_value);
            return Disposable.Empty;
        }
    }

    internal class Disposable : IDisposable
    {
        public static readonly IDisposable Empty = new Disposable();

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
