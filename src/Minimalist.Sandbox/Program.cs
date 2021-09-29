// See https://aka.ms/new-console-template for more information

using Minimalist.Reactive;

var s = new ReturnProducer<int>(1)
    .Where(x => x > 0)
    .Select(x => x.ToString())
    .Subscribe(new LogObserver<string>());

await Task.Delay(3000);
