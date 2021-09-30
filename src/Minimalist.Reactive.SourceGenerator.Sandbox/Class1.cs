using Minimalist.Reactive.Linq;

public partial class MyCoolClass
{
    public MyCoolClass()
    {
        //MyRxProperty.Subscribe();
    }

    public static void Main()
    {
    }

    [Rxify]
    public IObservable<string> DoSomething()
    {
        return Observable.Return(1)
            .Where(x => x > 0)
            .Select(x => x.ToString());
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class Rxify : Attribute
    {
    }
}