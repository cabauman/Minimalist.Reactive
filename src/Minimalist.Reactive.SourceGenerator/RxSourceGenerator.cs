using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Minimalist.Reactive.SourceGenerator
{
    [Generator]
    public class RxSourceGenerator : ISourceGenerator
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
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
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
            INamedTypeSymbol? attributeSymbol = context.Compilation.GetTypeByMetadataName("Minimalist.Reactive.RxifyAttribute");

            foreach (var group in syntaxReceiver.Candidates.GroupBy(method => method.Symbol.ContainingType))
            {
                var classSource = ProcessClass(group.Key, group.ToList());
                if (classSource != null)
                {
                    classSource = SyntaxFactory.ParseSyntaxTree(classSource).GetRoot().NormalizeWhitespace().ToFullString();
                    context.AddSource($"{group.Key.Name}.g.cs", SourceText.From(classSource, Encoding.UTF8));
                }
            }

            //            var className = "";
            //            var wherePredicate = "";
            //            var returnArg = "";
            //            var methods = new List<string>();
            //            foreach (var invocationExpression in syntaxReceiver.Candidates)
            //            {
            //                var model = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
            //                var symbol = model.GetSymbolInfo(invocationExpression).Symbol;

            //                if (symbol is not IMethodSymbol methodSymbol)
            //                {
            //                    continue;
            //                }

            //                className = methodSymbol.ContainingType.Name;
            //                string methodName = methodSymbol.ContainingType.Name;
            //                if (methodName == "Return")
            //                {
            //                    returnArg = invocationExpression.ArgumentList.Arguments[0].GetFirstToken().ValueText;
            //                    continue;
            //                }

            //                var args = methodSymbol.Parameters.Select(x => $"{x.Type.ToDisplayString()} {x.Name}");
            //                var argsString = string.Join(", ", args);
            //                //foreach (var argument in invocationExpression.ArgumentList.Arguments)
            //                //{

            //                //}

            //                string body = "";
            //                //if (methodName == "Return")
            //                //{
            //                //    body = "return value";
            //                //}
            //                if (methodName == "Where")
            //                {
            //                    body = "return predicate(value);";
            //                    wherePredicate = invocationExpression.ArgumentList.Arguments[0].ToString();
            //                }
            //                else if (methodName == "Select")
            //                {
            //                    body = "return selector(value);";
            //                }

            //                var methodTemplate = $@"
            //private bool {methodName}(int value, {argsString})
            //{{
            //{body}
            //}}
            //";
            //                methods.Add(methodTemplate);
            //            }

            //            var methodsString = string.Join("\n\n", methods);

            //            var source = $@"
            //using System;
            //using Minimalist.Reactive.Linq;
            //using Minimalist.Reactive.Disposables;
            //public partial class {className}
            //{{
            //    public IObservable<int> DoSomethingProperty {{ get; }} = new MyObservable();

            //    private class MyObservable : IObservable<int>
            //    {{
            //        public IDisposable Subscribe(IObserver<int> observer)
            //        {{
            //            var x = {returnArg};
            //            if (!Where(x, {wherePredicate}))
            //            {{
            //                observer.OnCompleted();
            //                return Disposable.Empty;
            //            }}
            //            observer.OnNext(x);
            //            observer.OnCompleted();
            //            return Disposable.Empty;
            //        }}

            //{methodsString}
            //    }}
            //}}";
            //context.AddSource($"MyCoolClass.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private string? ProcessClass(INamedTypeSymbol classSymbol, List<Data> methods)
        {
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                return null; //TODO: issue a diagnostic that it must be top level
            }

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            // begin building the generated source
            var source = new StringBuilder($@"
using System;
using Minimalist.Reactive.Linq;
using Minimalist.Reactive.Disposables;
namespace {namespaceName}
{{
    public partial class {classSymbol.Name}
    {{
");

            foreach (var method in methods)
            {
                ProcessMethod(source, method);
            }

            source.Append("} }");
            return source.ToString();
        }

        private void ProcessMethod(StringBuilder source, Data method)
        {
            var accessModifier = "public"; // methodSymbol.DeclaredAccessibility.ToString();
            var returnType = "int"; //methodSymbol.ReturnType.ToDisplayString();
            var methodName = method.Symbol.Name;
            var propertySource = $"{accessModifier} IObservable<{returnType}> {methodName}Property {{ get; }} = new {methodName}Observable();";
            source.AppendLine(propertySource);

            source.AppendLine($@"
    private class {methodName}Observable : IObservable<{returnType}>
    {{
        public IDisposable Subscribe(IObserver<{returnType}> observer)
        {{
");
            ProcessObservable(source, method.Symbol, method.Syntax);
            source.AppendLine("return Disposable.Empty; } }");
        }

        private void ProcessObservable(StringBuilder source, IMethodSymbol methodSymbol, MethodDeclarationSyntax methodSyntax)
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
                var expression = invocationExpression.Expression as MemberAccessExpressionSyntax;
                var methodName = expression?.Name.ToString();
                if (methodName == "Return")
                {
                    var arg = invocationExpression.ArgumentList.Arguments[0].GetFirstToken().ValueText;
                    source.AppendLine($"var x = {arg};");
                }
                if (methodName == "Where")
                {
                    var lambda = invocationExpression.ArgumentList.Arguments[0].Expression as LambdaExpressionSyntax;
                    var filter = lambda.Body.ToString();
                    source.AppendLine($"if ({filter}) {{ observer.OnNext(x); }}");
                }
            }

            source.AppendLine("observer.OnCompleted();");
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
