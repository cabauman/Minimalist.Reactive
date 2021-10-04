namespace Minimalist.Reactive.Testing
{
    /// <summary>
    /// Observer that records received notification messages and timestamps those.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    public interface ITestableObserver<T> : IObserver<T>
    {
        /// <summary>
        /// Gets recorded timestamped notification messages received by the observer.
        /// </summary>
        IList<Recorded<Notification<T>>> Messages { get; }
    }
}
