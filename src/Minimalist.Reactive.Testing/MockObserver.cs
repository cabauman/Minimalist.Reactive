namespace Minimalist.Reactive.Testing;

internal class MockObserver<T> : ITestableObserver<T>
{
    private readonly TestScheduler _scheduler;
    private readonly List<Recorded<Notification<T>>> _messages;

    public MockObserver(TestScheduler scheduler)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _messages = new List<Recorded<Notification<T>>>();
    }

    public void OnNext(T value)
    {
        _messages.Add(new Recorded<Notification<T>>(_scheduler.Clock, Notification.CreateOnNext(value)));
    }

    public void OnError(Exception exception)
    {
        _messages.Add(new Recorded<Notification<T>>(_scheduler.Clock, Notification.CreateOnError<T>(exception)));
    }

    public void OnCompleted()
    {
        _messages.Add(new Recorded<Notification<T>>(_scheduler.Clock, Notification.CreateOnCompleted<T>()));
    }

    public IList<Recorded<Notification<T>>> Messages
    {
        get { return _messages; }
    }
}
