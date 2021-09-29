namespace Minimalist.Reactive.Linq
{
    public static class ObservableExtensions
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
