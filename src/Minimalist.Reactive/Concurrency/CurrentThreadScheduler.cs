﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace Minimalist.Reactive.Concurrency;

public sealed class CurrentThreadScheduler : IScheduler
{
    private static readonly Lazy<CurrentThreadScheduler> StaticInstance = new Lazy<CurrentThreadScheduler>(() => new CurrentThreadScheduler());

    private CurrentThreadScheduler()
    {
    }

    /// <summary>
    /// Gets the singleton instance of the current thread scheduler.
    /// </summary>
    public static CurrentThreadScheduler Instance => StaticInstance.Value;

    [ThreadStatic]
    private static SchedulerQueue<TimeSpan>? _threadLocalQueue;

    [ThreadStatic]
    static Stopwatch? s_clock;

    [ThreadStatic]
    private static bool _running;

    private static SchedulerQueue<TimeSpan>? GetQueue() => _threadLocalQueue;

    private static void SetQueue(SchedulerQueue<TimeSpan>? newQueue)
    {
        _threadLocalQueue = newQueue;
    }

    private static TimeSpan Time
    {
        get
        {
            if (s_clock == null)
                s_clock = Stopwatch.StartNew();

            return s_clock.Elapsed;
        }
    }

    public DateTimeOffset Now => DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets a value that indicates whether the caller must call a Schedule method.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static bool IsScheduleRequired => !_running;

    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        return action(this, state);
    }

    /// <summary>
    /// Schedules an action to be executed after dueTime.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
    /// <param name="state">State passed to the action to be executed.</param>
    /// <param name="action">Action to be executed.</param>
    /// <param name="dueTime">Relative time after which to execute the action.</param>
    /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        SchedulerQueue<TimeSpan>? queue;

        // There is no timed task and no task is currently running
        if (!_running)
        {
            _running = true;

            if (dueTime > TimeSpan.Zero)
            {
                Thread.Sleep(dueTime);
            }

            // execute directly without queueing
            IDisposable d;
            try
            {
                d = action(this, state);
            }
            catch
            {
                SetQueue(null);
                _running = false;
                throw;
            }

            // did recursive tasks arrive?
            queue = GetQueue();

            // yes, run those in the queue as well
            if (queue != null)
            {
                try
                {
                    Trampoline.Run(queue);
                }
                finally
                {
                    SetQueue(null);
                    _running = false;
                }
            }
            else
            {
                _running = false;
            }

            return d;
        }

        queue = GetQueue();

        // if there is a task running or there is a queue
        if (queue == null)
        {
            queue = new SchedulerQueue<TimeSpan>(4);
            SetQueue(queue);
        }

        var dt = Time + Scheduler.Normalize(dueTime);

        // queue up more work
        var si = new ScheduledItem<TimeSpan, TState>(this, state, action, dt);
        queue.Enqueue(si);
        return si;
    }

    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        var due = Scheduler.Normalize(dueTime - Now);
        return Schedule(state, TimeSpan.Zero, action);
    }

    private static class Trampoline
    {
        public static void Run(SchedulerQueue<TimeSpan> queue)
        {
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                if (!item.IsCanceled)
                {
                    var wait = item.DueTime - Time;
                    if (wait.Ticks > 0)
                    {
                        Thread.Sleep(wait);
                    }

                    if (!item.IsCanceled)
                    {
                        item.Invoke();
                    }
                }
            }
        }
    }
}
