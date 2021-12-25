using System;

namespace Minimalist.Reactive.Disposables;

/// <summary>
/// Disposable resource with disposal state tracking.
/// </summary>
public interface ICancelable : IDisposable
{
    /// <summary>
    /// Gets a value that indicates whether the object is disposed.
    /// </summary>
    bool IsDisposed { get; }
}
