namespace Minimalist.Reactive.Disposables
{
    /// <summary>
    /// Represents a disposable resource which only allows a single assignment of its underlying disposable resource.
    /// If an underlying disposable resource has already been set, future attempts to set the underlying disposable resource will throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    public sealed class SingleAssignmentDisposable : ICancelable
    {
        private SingleAssignmentDisposableValue _current;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleAssignmentDisposable"/> class.
        /// </summary>
        public SingleAssignmentDisposable()
        {
        }

        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        /// </summary>
        public bool IsDisposed => _current.IsDisposed;

        /// <summary>
        /// Gets or sets the underlying disposable. After disposal, the result of getting this property is undefined.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="SingleAssignmentDisposable"/> has already been assigned to.</exception>
        public IDisposable? Disposable
        {
            get => _current.Disposable;
            set => _current.Disposable = value;
        }

        /// <summary>
        /// Disposes the underlying disposable.
        /// </summary>
        public void Dispose()
        {
            _current.Dispose();
        }
    }
}
