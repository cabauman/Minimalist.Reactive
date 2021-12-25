namespace Minimalist.Reactive.Disposables;

/// <summary>
/// Represents a disposable resource whose underlying disposable resource can be replaced by another disposable resource, causing automatic disposal of the previous underlying disposable resource.
/// </summary>
public sealed class SerialDisposable : ICancelable
{
    private SerialDisposableValue _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Reactive.Disposables.SerialDisposable"/> class.
    /// </summary>
    public SerialDisposable()
    {
    }

    /// <summary>
    /// Gets a value that indicates whether the object is disposed.
    /// </summary>
    public bool IsDisposed => _current.IsDisposed;

    /// <summary>
    /// Gets or sets the underlying disposable.
    /// </summary>
    /// <remarks>If the SerialDisposable has already been disposed, assignment to this property causes immediate disposal of the given disposable object. Assigning this property disposes the previous disposable object.</remarks>
    public IDisposable? Disposable
    {
        get => _current.Disposable;
        set => _current.Disposable = value;
    }

    /// <summary>
    /// Disposes the underlying disposable as well as all future replacements.
    /// </summary>
    public void Dispose()
    {
        _current.Dispose();
    }
}
