using Minimalist.Reactive;
using Minimalist.Reactive.Linq;
using Minimalist.Reactive.SourceGenerator;
using Minimalist.Reactive.SourceGenerator.Sandbox;

namespace Hello
{
    public partial class MyCoolClass
    {
        public MyCoolClass()
        {
            DoSomethingProperty.Subscribe(new LogObserver<int>());
        }

        public static void Main()
        {
            var x = new MyCoolClass();
        }

        [Rxify]
        public IObservable<int> DoSomething()
        {
            return Observable.Return(1).Where(x => x > 0);
        }
    }
}