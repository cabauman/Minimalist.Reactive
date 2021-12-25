namespace Minimalist.Reactive.Linq;

public static class ObservableExtensions
{
    public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        return new WhereOperator<T>(source, predicate);
    }

    public static IObservable<TResult> Select<T, TResult>(this IObservable<T> source, Func<T, TResult> selector)
    {
        return new SelectOperator<T, TResult>(source, selector);
    }

    public static IObservable<int> Count<T>(this IObservable<T> source)
    {
        return new CountOperator<T>(source);
    }

    public static IObservable<int> Count<T>(this IObservable<T> source, Func<T, bool> predicate)
    {
        return new PredicateCountOperator<T>(source, predicate);
    }

    public static IObservable<bool> All<T, TResult>(this IObservable<T> source, Func<T, bool> predicate)
    {
        return new AllOperator<T>(source, predicate);
    }

    public static IObservable<Notification<TSource>> Materialize<TSource>(this IObservable<TSource> source)
    {
        return new MaterializeOperator<TSource>(source);
    }

    public static IEnumerable<TSource> ToEnumerable<TSource>(this IObservable<TSource> source)
    {
        return new AnonymousEnumerable<TSource>(() => source.GetEnumerator());
    }

    public static IEnumerator<TSource> GetEnumerator<TSource>(this IObservable<TSource> source)
    {
        var e = new GetEnumerator<TSource>();
        return e.Run(source);
    }
}
