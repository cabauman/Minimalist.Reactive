namespace Minimalist.Reactive.Sandbox;

public class LogObserver<T> : IObserver<T>
{
    public void OnNext(T value)
    {
        Console.WriteLine($"OnNext: {value}");
    }

    public void OnCompleted()
    {
        Console.WriteLine("OnCompleted");
    }

    public void OnError(Exception error)
    {
        Console.WriteLine($"OnError: {error}");
    }
}
