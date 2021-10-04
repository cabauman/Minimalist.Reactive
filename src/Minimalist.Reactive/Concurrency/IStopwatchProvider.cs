namespace Minimalist.Reactive.Concurrency
{
    /// <summary>
    /// Provider for <see cref="IStopwatch"/> objects.
    /// </summary>
    public interface IStopwatchProvider
    {
        /// <summary>
        /// Starts a new stopwatch object.
        /// </summary>
        /// <returns>New stopwatch object; started at the time of the request.</returns>
        IStopwatch StartStopwatch();
    }
}
