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
            //var where = new Where();
            //ISourceCreator sourceCreator = new StringBuilderSourceCreator();
            //var source = where.Accept(sourceCreator);
            //IOperatorData[] operatorData = new[] { where };
            //foreach (var operatorDatum in operatorData)
            //{
            //    operatorDatum.Accept(sourceCreator);
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

            var sourceCreator = new StringBuilderSourceCreator();
            foreach (var group in syntaxReceiver.Candidates.GroupBy(method => method.Symbol.ContainingType))
            {
                var classData = ProcessClass(group.Key, group.ToList(), compilation);
                var classSource = sourceCreator.Create(classData!);
                context.AddSource($"{group.Key.Name}.g.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private ClassDatum? ProcessClass(INamedTypeSymbol classSymbol, List<Data> methods, Compilation compilation)
        {
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                // TODO: Issue a diagnostic that it must be top level.
                return null;
            }

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            string className = classSymbol.Name;

            var classContents = new List<ClassContent>();
            foreach (var method in methods)
            {
                classContents.Add(ProcessMethod(method, compilation, className));
            }

            return new ClassDatum
            {
                NamespaceName = namespaceName,
                ClassName = className,
                ClassContents = classContents,
            };
        }

        private ClassContent ProcessMethod(Data method, Compilation compilation, string className)
        {
            var accessModifier = method.Symbol.DeclaredAccessibility.ToFriendlyString();
            var returnType = method.Symbol.ReturnType.ToDisplayString();
            var genericReturnType = (method.Symbol.ReturnType as INamedTypeSymbol).TypeArguments[0];
            var methodName = method.Symbol.Name;
            var propertySource = $"{accessModifier} IObservable<{genericReturnType.ToDisplayString()}> {methodName}Property {{ get; }} = new {methodName}Observable();";

            return new ClassContent
            {
                PropertyDatum = new ObservablePropertyDatum { Name = $"{methodName}Property", OriginalMethodName = methodName, GenericType = method.Symbol.ReturnType, Accessibility = method.Symbol.DeclaredAccessibility },
                ClassDatum = ProcessObservable(method.Symbol, method.Syntax, compilation, genericReturnType, className),
            };
        }

        private NestedClassDatum ProcessObservable(IMethodSymbol methodSymbol, MethodDeclarationSyntax methodSyntax, Compilation compilation, ITypeSymbol genericObserverType, string className)
        {
            var model = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            var invocationExpressions = new List<InvocationExpressionSyntax>();
            InvocationExpressionSyntax current = methodSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            while (current != null)
            {
                invocationExpressions.Add(current);
                current = ((MemberAccessExpressionSyntax)current.Expression).Expression as InvocationExpressionSyntax;
            }
            invocationExpressions.Reverse();
            var operatorFields = new List<FieldDatum>();
            var operatorData = new List<IOperatorDatum>();
            var operatorMethods = new List<MethodDatum> { new MethodDatum { Name = "Run", Accessibility = "public", ReturnType = "void", ParameterData = Array.Empty<ParameterDatum>(), OperatorData = operatorData } };
            int counter = 0;
            foreach (var invocationExpression in invocationExpressions)
            {
                var operatorMethodSymbol = model.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
                var parameters = operatorMethodSymbol!.Parameters;
                var arguments = invocationExpression.ArgumentList.Arguments;
                var parameterData = new List<ArgDatum>();
                for (int i = 0; i < arguments.Count; i++)
                {
                    var current1 = arguments[i].Expression;
                    while (current1 is MemberAccessExpressionSyntax memberAccessExpression)
                    {
                        current1 = memberAccessExpression.Expression;
                    }
                    var isParentMember = false;
                    var argSymbol = model.GetSymbolInfo(current1).Symbol;
                    if (argSymbol is not null && argSymbol.ContainingType.Name.Equals(className))
                    {
                        isParentMember = true;
                    }
                    var parameterDatum = new ArgDatum { ParameterName = parameters[i].Name, Type = parameters[i].Type, Expression = arguments[i].Expression, IsMemberOfTargetClass = isParentMember };
                    parameterData.Add(parameterDatum);
                }

                var operatorGenericReturnType = operatorMethodSymbol.TypeArguments[0].ToDisplayString();

                var operatorDatum = OperatorInfoMap.GetInfo(operatorMethodSymbol.Name, parameterData);
                operatorFields.AddRange(operatorDatum.Fields);
                operatorData.Add(operatorDatum);
                if (operatorDatum.RequiresScheduling)
                {
                    operatorData = new List<IOperatorDatum>();
                    var methodDatum = new MethodDatum { Name = $"Tick{counter++}", Accessibility = "private", ReturnType = "void", ParameterData = new[] { new ParameterDatum { Name = "x0", Type = operatorGenericReturnType } }, OperatorData = operatorData };
                    operatorMethods.Add(methodDatum);
                }
            }

            // Add one operatorDatum that calls observer.OnNext.
            operatorData.Add(new ObserverOnNext());

            return new NestedClassDatum
            {
                ClassName = $"{methodSymbol.Name}Observable",
                GenericType = genericObserverType.ToDisplayString(),
                Fields = operatorFields.ToArray(),
                Methods = operatorMethods.ToArray(),
            };
        }
    }

    internal static class OperatorInfoMap
    {
        public static IOperatorDatum GetInfo(string operatorName, List<ArgDatum> argData)
        {
            return operatorName switch
            {
                "Return" => new Return(argData),
                "Where" => new Where(argData),
                "Select" => new Select(argData),
                _ => throw new NotSupportedException(),
            };
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
