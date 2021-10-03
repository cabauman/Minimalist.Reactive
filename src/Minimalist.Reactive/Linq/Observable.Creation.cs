using Minimalist.Reactive.Linq.Creation;

namespace Minimalist.Reactive.Linq
{
    public static class Observable
    {
        /// <summary>
        /// Returns an observable sequence that contains a single element.
        /// </summary>
        /// <typeparam name="TResult">The type of the element that will be returned in the produced sequence.</typeparam>
        /// <param name="value">Single element in the resulting observable sequence.</param>
        /// <returns>An observable sequence containing the single specified element.</returns>
        public static IObservable<TResult> Return<TResult>(TResult value)
        {
            return new ReturnImmediate<TResult>(value);
        }
    }
}
