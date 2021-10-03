using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Linq.Creation
{
    internal sealed class ReturnOperator<TSource> : IObservable<TSource>
    {
        private readonly TSource _value;
        private readonly IScheduler _scheduler;

        public ReturnOperator(TSource value, IScheduler scheduler)
        {
            _value = value;
            _scheduler = scheduler;
        }

        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            return _scheduler.Schedule(
                (@this: this, observer),
                static (scheduler, state) => state.@this.Invoke(state.observer));
        }

        private IDisposable Invoke(IObserver<TSource> observer)
        {
            observer.OnNext(_value);
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }

    internal sealed class ReturnImmediate<TSource> : IObservable<TSource>
    {
        private readonly TSource _value;

        public ReturnImmediate(TSource value)
        {
            _value = value;
        }

        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            observer.OnNext(_value);
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}
