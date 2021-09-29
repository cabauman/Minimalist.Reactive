using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Linq.Creation
{
    internal sealed class ReturnOperator<T> : IObservable<T>
    {
        private readonly T _value;

        public ReturnOperator(T value)
        {
            _value = value;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(_value);
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}
