// See https://aka.ms/new-console-template for more information

using Minimalist.Reactive.Sandbox;
using Minimalist.Reactive.Linq;

var o = Observable.Return(1)
    .Where(x => x < 0)
    .Select(x => x.ToString())
    .Count();
o.Subscribe(new LogObserver<int>());
await Task.Delay(1000);
o.Subscribe(new LogObserver<int>());

await Task.Delay(1000);
