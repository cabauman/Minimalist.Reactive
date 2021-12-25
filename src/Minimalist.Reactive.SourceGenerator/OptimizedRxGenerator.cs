using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Minimalist.Reactive.SourceGenerator.Blueprints;
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

        private ClassDatum? ProcessClass(INamedTypeSymbol classSymbol, List<RxifyInput> methods, Compilation compilation)
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

        private ClassContent ProcessMethod(RxifyInput method, Compilation compilation, string className)
        {
            if (method.Symbol.ReturnType is not INamedTypeSymbol returnType)
            {
                return null;
            }
            var genericReturnType = returnType.TypeArguments[0];
            var methodName = method.Symbol.Name;

            return new ClassContent
            {
                PropertyDatum = new ObservablePropertyDatum
                {
                    Name = $"{methodName}Property",
                    OriginalMethodName = methodName,
                    GenericType = method.Symbol.ReturnType,
                    Accessibility = method.Symbol.DeclaredAccessibility
                },
                ClassDatum = ProcessObservable(method.Symbol, method.Syntax, compilation, genericReturnType, className),
            };
        }

        private CustomObservableClass ProcessObservable(IMethodSymbol methodSymbol, MethodDeclarationSyntax methodSyntax, Compilation compilation, ITypeSymbol genericObserverType, string className)
        {
            var model = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            var invocationExpressions = new List<InvocationExpressionSyntax>();
            var current = methodSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            while (current != null)
            {
                invocationExpressions.Add(current);
                if (current.Expression is MemberAccessExpressionSyntax mae)
                {
                    current = mae.Expression as InvocationExpressionSyntax;
                }
                else
                {
                    current = null;
                }
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
                    var isMemberOfTargetClass = false;
                    var argSymbol = model.GetSymbolInfo(current1).Symbol;
                    if (argSymbol is not null && argSymbol.ContainingType.Name.Equals(className))
                    {
                        isMemberOfTargetClass = true;
                    }
                    var parameterDatum = new ArgDatum { ParameterName = parameters[i].Name, Type = parameters[i].Type, Expression = arguments[i].Expression, IsMemberOfTargetClass = isMemberOfTargetClass };
                    parameterData.Add(parameterDatum);
                }

                var operatorGenericReturnType = operatorMethodSymbol.TypeArguments[0].ToDisplayString();

                var operatorDatum = OperatorInfoMap.GetInfo(operatorMethodSymbol.Name, parameterData);
                operatorFields.AddRange(operatorDatum.Fields);
                operatorData.Add(operatorDatum);
                if (operatorDatum.RequiresScheduling)
                {
                    operatorData = new List<IOperatorDatum>();
                    var methodDatum = new MethodDatum
                    {
                        Name = $"Tick{counter++}",
                        Accessibility = "private",
                        ReturnType = "void",
                        ParameterData = new[]
                        {
                            new ParameterDatum
                            {
                                Name = "x0",
                                Type = operatorGenericReturnType
                            }
                        },
                        OperatorData = operatorData
                    };
                    operatorMethods.Add(methodDatum);
                }
            }

            // Add one operatorDatum that calls observer.OnNext.
            operatorData.Add(new ObserverOnNext());

            return new CustomObservableClass
            {
                ClassName = $"{methodSymbol.Name}Observable",
                GenericType = genericObserverType.ToDisplayString(),
                Fields = operatorFields.ToArray(),
                Methods = operatorMethods.ToArray(),
            };
        }
    }
}
