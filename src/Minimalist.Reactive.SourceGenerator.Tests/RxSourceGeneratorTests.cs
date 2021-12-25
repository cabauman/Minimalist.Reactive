using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Minimalist.Reactive.SourceGenerator.Tests;

public class RxSourceGeneratorTests
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WhenChangedGeneratorTests"/> class.
    /// </summary>
    /// <param name="testContext">The test context.</param>
    public RxSourceGeneratorTests(ITestOutputHelper testContext)
    {
        TestContext = testContext;
    }

    /// <summary>
    /// Gets the test context.
    /// </summary>
    public ITestOutputHelper TestContext { get; }

    [Fact]
    public void Test1()
    {
        var sources = new[] { Source };
        var compilation = CompilationUtil.CreateCompilation(sources);
        var newCompilation = CompilationUtil.RunGenerators(compilation, out var generatorDiagnostics, new OptimizedRxGenerator());

        var generatedSource = string.Join(Environment.NewLine, newCompilation.SyntaxTrees.Select(x => x.ToString()));
        generatedSource = SyntaxFactory.ParseSyntaxTree(generatedSource).GetRoot().NormalizeWhitespace().ToFullString();
        TestContext.WriteLine(generatedSource);

        var compilationDiagnostics = newCompilation.GetDiagnostics();
        var compilationErrors = compilationDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage()).ToList();

        if (compilationErrors.Count > 0)
        {
            throw new InvalidOperationException(string.Join('\n', compilationErrors));
        }

        Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
        Assert.Empty(compilationDiagnostics.Where(x => x.Severity > DiagnosticSeverity.Warning));

        var assembly = GetAssembly(newCompilation);
        var generatedType = assembly.GetType("Hello.MyCoolClass");
        var instance = CreateInstance(generatedType ?? throw new InvalidOperationException("Failed to find generated type."));
        var generatedProperty = GetProperty(instance, "DoSomethingProperty");
        var o = new MyObserver(TestContext);
        generatedProperty.Subscribe(o);
        Assert.Equal(1, o.Value);
    }

    public class MyObserver : IObserver<int>
    {
        private ITestOutputHelper _logger;

        public int Value { get; set; }

        public MyObserver(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        public void OnCompleted()
        {
            _logger.WriteLine("completed");
        }

        public void OnError(Exception error)
        {
            _logger.WriteLine("error: " + error);
        }

        public void OnNext(int value)
        {
            Value = value;
            _logger.WriteLine("value: " + value);
        }
    }

    private static object CreateInstance(Type type) =>
        Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null, null) ?? throw new InvalidOperationException("The value of the type cannot be null");

    private static Assembly GetAssembly(Compilation compilation)
    {
        using var ms = new MemoryStream();
        compilation.Emit(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }

    internal static object? GetMethod(object target, string methodName) =>
        target.GetType().InvokeMember(
            methodName,
            BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null,
            target,
            Array.Empty<object>());

    public static IObservable<int> GetProperty(object target, string propertyName)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or whitespace.", nameof(propertyName));
        }

        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var value = prop.GetValue(target);
        var result = value as IObservable<int>;
        return result;
    }

    private const string Source =
        @"
using System;
using Minimalist.Reactive;
using Minimalist.Reactive.Linq;
using Minimalist.Reactive.Concurrency;
namespace Hello
{
    public partial class MyCoolClass
    {
        private IScheduler _returnScheduler;
        private SchedulerContainer _schedulerContainer = null;

        public MyCoolClass()
        {
            _returnScheduler = Scheduler.Immediate;
            //var x = DoSomethingProperty.Subscribe(new LogObserver<int>());
        }

        public static void Main()
        {
            var x = new MyCoolClass();
        }

        [Rxify]
        public IObservable<int> DoSomething()
        {
            //GetScheduler()
            //this._schedulerContainer.Scheduler
            return Observable.Return(1, _returnScheduler).Where(x => x > 0);
        }
        
        private IScheduler GetScheduler()
        {
            return _returnScheduler;
        }
    }
}

public class SchedulerContainer
{
    public IScheduler Scheduler = null;
}

namespace Hello
{
    public class LogObserver<T> : IObserver<T>
    {
        public void OnNext(T value)
        {
            Console.WriteLine($""OnNext: {{value}}"");
        }

        public void OnCompleted()
        {
            Console.WriteLine(""OnCompleted"");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine($""OnError: {error}"");
        }
    }
}
";
}
