using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Minimalist.Reactive.SourceGenerator.OperatorData;
using Minimalist.Reactive.SourceGenerator.SourceCreator;
using System.Text;

namespace Minimalist.Reactive.SourceGenerator
{
    [Generator]
    public class OptimizedRxGenerator : ISourceGenerator
    {
        private const string AttributeText = @"
using System;
namespace Minimalist.Reactive
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RxifyAttribute : Attribute
    {
    }
}";

        public void Initialize(GeneratorInitializationContext context)
        {
            var where = new Where();
            ISourceCreator sourceCreator = new StringBuilderSourceCreator();
            var source = where.Accept(sourceCreator);
            IOperatorData[] operatorData = new[] { where };
            foreach (var operatorDatum in operatorData)
            {
                operatorDatum.Accept(sourceCreator);
            }

            context.RegisterForPostInitialization((i) => i.AddSource("RxifyAttribute", AttributeText));
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver2());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiver2 syntaxReceiver)
            {
                return;
            }

            Compilation compilation = context.Compilation;

            foreach (var group in syntaxReceiver.Candidates.GroupBy(method => method.Symbol.ContainingType))
            {
                var classData = ProcessClass(group.Key, group.ToList());
            }
        }

        private string? ProcessClass(INamedTypeSymbol classSymbol, List<Data> methods)
        {
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                // TODO: Issue a diagnostic that it must be top level.
                return null;
            }

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            string className = classSymbol.Name;

            foreach (var method in methods)
            {
                ProcessMethod(method);
            }
            return "";
        }

        private void ProcessMethod(Data method)
        {
            var accessModifier = method.Symbol.DeclaredAccessibility.ToFriendlyString();
            var returnType = method.Symbol.ReturnType.ToDisplayString();
            var methodName = method.Symbol.Name;
            var propertySource = $"{accessModifier} IObservable<{returnType}> {methodName}Property {{ get; }} = new {methodName}Observable();";
            ProcessObservable(method.Symbol, method.Syntax);
        }

        private void ProcessObservable(IMethodSymbol methodSymbol, MethodDeclarationSyntax methodSyntax)
        {
            var invocationExpressions = new List<InvocationExpressionSyntax>();
            InvocationExpressionSyntax current = methodSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            while (current != null)
            {
                invocationExpressions.Add(current);
                current = current.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            }
            invocationExpressions.Reverse();
            foreach (var invocationExpression in invocationExpressions)
            {
            }
        }
    }
}

//private class DoSomethingObservable : IObservable<int>
//{
//    public IDisposable Subscribe(IObserver<int> observer)
//    {
//        while (true)
//        {
//            var x = 1;
//            if (!(x > 0))
//            {
//                continue;
//            }
//            var s1 = x.ToString();
//            if (!(s1.Length > 0))
//            {
//                continue;
//            }
//            observer.OnNext(x);
//        }
//        observer.OnCompleted();
//        return Disposable.Empty;
//    }
//}

//private class DoSomethingObservable : IObservable<int>
//{
//    public IDisposable Subscribe(IObserver<int> observer)
//    {
//        scheduler.Schedule(TimerTick, period);
//    }

//    private void TimerTick(int value)
//    {
//        if (!(value > 0))
//        {
//            return;
//        }
//        observeOnScheduler1.Schedule(ObsverveOnTick1);
//    }

//    private void ObsverveOnTick1()
//    {
//        observer.OnNext(x);
//    }
//}
