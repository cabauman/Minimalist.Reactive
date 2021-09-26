using System;

namespace Minimalist.Reactive
{
    internal class Program
    {
        public void Execute()
        {
            var s = new ReturnProducer<int>(1)
                .Where(x => x > 0)
                .Select(x => x.ToString());
        }
    }
}
