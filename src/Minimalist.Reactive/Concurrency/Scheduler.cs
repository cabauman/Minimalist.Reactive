namespace Minimalist.Reactive.Concurrency;

public static partial class Scheduler
{
    /// <summary>
    /// Normalizes the specified <see cref="TimeSpan"/> value to a positive value.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> value to normalize.</param>
    /// <returns>The specified TimeSpan value if it is zero or positive; otherwise, <see cref="TimeSpan.Zero"/>.</returns>
    public static TimeSpan Normalize(TimeSpan timeSpan) => timeSpan.Ticks < 0 ? TimeSpan.Zero : timeSpan;

    /// <summary>
    /// Gets a scheduler that schedules work immediately on the current thread.
    /// </summary>
    public static ImmediateScheduler Immediate => ImmediateScheduler.Instance;

    /// <summary>
    /// Gets a scheduler that schedules work as soon as possible on the current thread.
    /// </summary>
    public static CurrentThreadScheduler CurrentThread => CurrentThreadScheduler.Instance;
}
