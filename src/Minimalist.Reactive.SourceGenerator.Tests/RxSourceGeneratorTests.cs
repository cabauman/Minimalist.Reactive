using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Minimalist.Reactive.SourceGenerator.Tests
{
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
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
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
            return Observable.Return(1, Scheduler.CurrentThread).Where(x => x < 0);
        }
    }
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
}