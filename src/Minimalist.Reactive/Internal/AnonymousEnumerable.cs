using System.Collections;

namespace Minimalist.Reactive
{
    internal sealed class AnonymousEnumerable<T> : IEnumerable<T>
    {
        private readonly Func<IEnumerator<T>> _getEnumerator;

        public AnonymousEnumerable(Func<IEnumerator<T>> getEnumerator)
        {
            _getEnumerator = getEnumerator;
        }

        public IEnumerator<T> GetEnumerator() => _getEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _getEnumerator();
    }
}
