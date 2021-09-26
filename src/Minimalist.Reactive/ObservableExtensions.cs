using Minimalist.Reactive.Linq.ObservableImpl;

namespace Minimalist.Reactive
{
    internal static class ObservableExtensions
    {
        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, bool> predicate)
        {
            return new WhereOperator<T>(source, predicate);
        }

        public static IObservable<TResult> Select<T, TResult>(this IObservable<T> source, Func<T, TResult> selector)
        {
            return new SelectOperator<T, TResult>(source, selector);
        }
    }
}
