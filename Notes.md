Desired result examples.

```
private class DoSomethingObservable : IObservable<int>
{
    public IDisposable Subscribe(IObserver<int> observer)
    {
        while (true)
        {
            var x = 1;
            if (!(x > 0))
            {
                continue;
            }
            var s1 = x.ToString();
            if (!(s1.Length > 0))
            {
                continue;
            }
            observer.OnNext(x);
        }
        observer.OnCompleted();
        return Disposable.Empty;
    }
}

private class DoSomethingObservable : IObservable<int>
{
    public IDisposable Subscribe(IObserver<int> observer)
    {
        scheduler.Schedule(TimerTick, period);
    }

    private void TimerTick(int value)
    {
        if (!(value > 0))
        {
            return;
        }
        observeOnScheduler1.Schedule(ObsverveOnTick1);
    }

    private void ObsverveOnTick1()
    {
        observer.OnNext(x);
    }
}
```