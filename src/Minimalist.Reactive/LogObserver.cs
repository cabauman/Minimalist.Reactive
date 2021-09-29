using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimalist.Reactive
{
    public class LogObserver<T> : IObserver<T>
    {
        public void OnNext(T value)
        {
            Console.WriteLine($"OnNext: {value}");
        }

        public void OnCompleted()
        {
            Console.WriteLine("OnCompleted");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine($"OnError: {error}");
        }
    }
}
