using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.OperatorData;
using Minimalist.Reactive.SourceGenerator.SourceCreator;
using System.Text;

namespace Minimalist.Reactive.SourceGenerator;

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
            var targetClassBlueprint = ProcessClass(group.Key, group.ToList(), compilation);
            if (targetClassBlueprint == null)
            {
                continue;
            }

            var semanticModelProvider = new SemanticModelProvider(compilation);
            var operatorLogicExtractor = new OperatorLogicExtractor(semanticModelProvider);
            var classInfoExtractor = new ClassDatumExtractor(operatorLogicExtractor);
            var classBlueprintCreator = new ClassBlueprintCreator();

            var classInfo = classInfoExtractor.Extract(group.Key, group.ToList());
            var classBlueprint = classBlueprintCreator.Create(classInfo);
            var classSource = sourceCreator.Create(targetClassBlueprint);

            context.AddSource($"{group.Key.Name}.g.cs", SourceText.From(classSource, Encoding.UTF8));
        }
    }

    private TargetClassBlueprint? ProcessClass(INamedTypeSymbol classSymbol, IReadOnlyList<RxifyInput> methods, Compilation compilation)
    {
        if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
        {
            // TODO: Issue a diagnostic that it must be top level.
            return null;
        }

        string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        string className = classSymbol.Name;

        var classContents = new List<TargetClassComponentBlueprint>();
        foreach (var method in methods)
        {
            classContents.Add(ProcessMethod(method, compilation, className));
        }

        return new TargetClassBlueprint
        {
            NamespaceName = namespaceName,
            ClassName = className,
            Components = classContents,
            //Fields = fields,
            //Properties = properties,
            //Classes = classes,
        };
    }

    private TargetClassComponentBlueprint ProcessMethod(RxifyInput method, Compilation compilation, string className)
    {
        if (method.Symbol.ReturnType is not INamedTypeSymbol returnType)
        {
            return null;
        }

        var genericReturnType = returnType.TypeArguments[0];
        var methodName = method.Symbol.Name;

        return new TargetClassComponentBlueprint
        {
            PropertyDatum = new TargetClassPropertyBlueprint
            {
                Name = $"{methodName}Property",
                OriginalMethodName = methodName,
                // TODO: Handle OutOfRangeException
                BackingFieldName = $"_{char.ToLower(methodName[0]) + methodName.Substring(1)}",
                ReturnType = method.Symbol.ReturnType,
                //InstanceTypeName = $"{methodName}Observable",
                Accessibility = method.Symbol.DeclaredAccessibility
            },
            ClassDatum = ProcessObservable(method.Symbol, method.Syntax, compilation, genericReturnType, className),
        };
    }

    private ObservableClassBlueprint ProcessObservable(
        IMethodSymbol methodSymbol,
        MethodDeclarationSyntax methodSyntax,
        Compilation compilation,
        ITypeSymbol genericTypeArgument,
        string targetClassName)
    {
        var model = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
        var invocationExpressions = ExtractInvocationExpressions(methodSyntax);
        var operatorFields = new List<ObservableClassFieldBlueprint>();
        var operatorLogicList = new List<IOperatorLogic>();
        var operatorMethods = new List<ObservableClassMethodBlueprint>
            {
                new ObservableClassMethodBlueprint
                {
                    Name = "Run",
                    Accessibility = "public",
                    ReturnType = "void",
                    Parameters = Array.Empty<ObservableClassMethodParameterBlueprint>(),
                    OperatorLogicItems = operatorLogicList
                }
            };

        int counter = 0;
        foreach (var invocationExpression in invocationExpressions)
        {
            if (model.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol operatorMethodSymbol)
            {
                throw new InvalidOperationException("Expected invocationExpression to have an IMethodSymbol");
            }

            var parameterSymbols = operatorMethodSymbol.Parameters;
            var argumentSyntaxList = invocationExpression.ArgumentList.Arguments;
            var operatorArgumentList = ExtractOperatorArguments(targetClassName, model, parameterSymbols, argumentSyntaxList);
            var operatorLogic = OperatorLogicFactory.Create(operatorMethodSymbol.Name, operatorMethodSymbol.TypeArguments[0].ToDisplayString(), operatorArgumentList);
            operatorFields.AddRange(operatorLogic.Fields);
            operatorLogicList.Add(operatorLogic);

            if (operatorLogic.RequiresScheduling)
            {
                var operatorGenericReturnType = operatorMethodSymbol.TypeArguments[0].ToDisplayString();
                operatorLogicList = new List<IOperatorLogic>();
                var observableClassMethodBlueprint = new ObservableClassMethodBlueprint
                {
                    Name = $"Tick{counter++}",
                    Accessibility = "private",
                    ReturnType = "void",
                    Parameters = new[]
                    {
                            new ObservableClassMethodParameterBlueprint
                            {
                                Name = "x0",
                                Type = operatorGenericReturnType
                            }
                        },
                    OperatorLogicItems = operatorLogicList
                };

                operatorMethods.Add(observableClassMethodBlueprint);
            }
        }

        operatorLogicList.Add(new ObserverOnNext());

        return new ObservableClassBlueprint
        {
            ClassName = $"{methodSymbol.Name}Observable",
            GenericTypeArgument = genericTypeArgument.ToDisplayString(),
            Fields = operatorFields.ToArray(),
            Methods = operatorMethods.ToArray(),
        };
    }

    private ObservableClassBlueprint GetObservableClassBlueprint(
        IReadOnlyList<IOperatorLogic> operatorLogicItems,
        //MethodDeclarationSyntax methodSyntax,
        //Compilation compilation,
        ITypeSymbol genericTypeArgument,
        //string targetClassName
        IMethodSymbol methodSymbol)
    {
        //var operatorLogicItems = ProcessObservable2(methodSyntax, compilation, targetClassName);
        return new ObservableClassBlueprint
        {
            ClassName = $"{methodSymbol.Name}Observable",
            GenericTypeArgument = genericTypeArgument.ToDisplayString(),
            Fields = GetFieldBlueprints(operatorLogicItems),
            Methods = GetMethodBlueprints(operatorLogicItems),
        };
    }

    private IReadOnlyList<ObservableClassFieldBlueprint> GetFieldBlueprints(IReadOnlyList<IOperatorLogic> operatorLogicItems)
    {
        var blueprints = new List<ObservableClassFieldBlueprint>();
        foreach (var operatorLogic in operatorLogicItems)
        {
            blueprints.AddRange(operatorLogic.Fields);
        }

        return blueprints;
    }

    private IReadOnlyList<ObservableClassMethodBlueprint> GetMethodBlueprints(IReadOnlyList<IOperatorLogic> operatorLogicItems)
    {
        var operatorLogicToIncludeInMethod = new List<IOperatorLogic>();
        var methodBlueprints = new List<ObservableClassMethodBlueprint>
            {
                new ObservableClassMethodBlueprint
                {
                    Name = "Run",
                    Accessibility = "public",
                    ReturnType = "void",
                    Parameters = Array.Empty<ObservableClassMethodParameterBlueprint>(),
                    OperatorLogicItems = operatorLogicToIncludeInMethod,
                }
            };

        var counter = 0;
        foreach (var operatorLogic in operatorLogicItems)
        {
            operatorLogicToIncludeInMethod.Add(operatorLogic);

            if (operatorLogic.RequiresScheduling)
            {
                var genericTypeArgument = operatorLogic.GenericTypeArgument;
                operatorLogicToIncludeInMethod = new List<IOperatorLogic>();
                var methodBlueprint = new ObservableClassMethodBlueprint
                {
                    Name = $"Tick{counter++}",
                    Accessibility = "private",
                    ReturnType = "void",
                    Parameters = new[]
                    {
                            new ObservableClassMethodParameterBlueprint
                            {
                                Name = "x0",
                                Type = genericTypeArgument
                            }
                        },
                    OperatorLogicItems = operatorLogicToIncludeInMethod,
                };

                methodBlueprints.Add(methodBlueprint);
            }
        }

        return methodBlueprints;
    }

    private IReadOnlyList<IOperatorLogic> GetOperatorLogicItems(
        MethodDeclarationSyntax methodSyntax,
        Compilation compilation,
        string targetClassName)
    {
        var model = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
        var invocationExpressions = ExtractInvocationExpressions(methodSyntax);
        var operatorLogicItems = new List<IOperatorLogic>();

        foreach (var invocationExpression in invocationExpressions)
        {
            var operatorSymbol = model.GetSymbolInfo(invocationExpression).Symbol;
            if (operatorSymbol is not IMethodSymbol operatorMethodSymbol)
            {
                throw new InvalidOperationException("Expected invocationExpression to have an IMethodSymbol");
            }

            var parameterSymbols = operatorMethodSymbol.Parameters;
            var argumentSyntaxItems = invocationExpression.ArgumentList.Arguments;
            var operatorArguments = ExtractOperatorArguments(targetClassName, model, parameterSymbols, argumentSyntaxItems);
            var genericTypeArgument = operatorMethodSymbol.TypeArguments[0].ToDisplayString();
            var operatorLogic = OperatorLogicFactory.Create(operatorMethodSymbol.Name, genericTypeArgument, operatorArguments);
            operatorLogicItems.Add(operatorLogic);
        }

        operatorLogicItems.Add(new ObserverOnNext());

        return operatorLogicItems;
    }

    private static List<OperatorArgument> ExtractOperatorArguments(
        string targetClassName,
        SemanticModel model,
        IReadOnlyList<IParameterSymbol> parameterSymbols,
        IReadOnlyList<ArgumentSyntax> argumentSyntaxList)
    {
        var operatorArgumentList = new List<OperatorArgument>();
        for (int i = 0; i < argumentSyntaxList.Count; i++)
        {
            bool doesOriginateFromTargetClass = DoesOriginateFromTargetClass(targetClassName, model, argumentSyntaxList[i]);
            var operatorArgument = new OperatorArgument
            {
                ParameterName = parameterSymbols[i].Name,
                Type = parameterSymbols[i].Type,
                Expression = argumentSyntaxList[i].Expression,
                DoesOriginateFromTargetClass = doesOriginateFromTargetClass
            };
            operatorArgumentList.Add(operatorArgument);
        }

        return operatorArgumentList;
    }

    private static bool DoesOriginateFromTargetClass(string targetClassName, SemanticModel model, ArgumentSyntax argumentSyntax)
    {
        // TODO: Extract to "get root" utility method.
        // TODO: Handle method access too e.g. InvocationExpression.
        var current = argumentSyntax.Expression;
        while (current is MemberAccessExpressionSyntax memberAccessExpression)
        {
            current = memberAccessExpression.Expression;
        }

        var isMemberOfTargetClass = false;
        var argumentSymbol = model.GetSymbolInfo(current).Symbol;
        if (argumentSymbol is not null && argumentSymbol.ContainingType.Name.Equals(targetClassName))
        {
            isMemberOfTargetClass = true;
        }

        return isMemberOfTargetClass;
    }

    private static List<InvocationExpressionSyntax> ExtractInvocationExpressions(MethodDeclarationSyntax methodSyntax)
    {
        var invocationExpressions = new List<InvocationExpressionSyntax>();
        // TODO: Be more restrictive for now by getting the first line of the body
        // instead of the DescendantNodes() way.
        var current = methodSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();

        while (current != null)
        {
            invocationExpressions.Add(current);
            if (current.Expression is MemberAccessExpressionSyntax methodAccessExpression)
            {
                current = methodAccessExpression.Expression as InvocationExpressionSyntax;
            }
            else
            {
                current = null;
            }
        }

        invocationExpressions.Reverse();
        return invocationExpressions;
    }

    // FullyQualifyMemberAccess
    private static IReadOnlyList<string> SanitizeArguments(string fieldNameOfTypeContainerClass, IReadOnlyList<OperatorArgument> operatorArguments)
    {
        var results = new List<string>();
        foreach (var argument in operatorArguments)
        {
            var argumentName = argument.Expression.ToString();
            // TODO: Call DoesOriginateFromTargetClass as a method here.
            if (argument.DoesOriginateFromTargetClass)
            {
                // TODO: Change ".this" to a const.
                if (argumentName.StartsWith("this."))
                {
                    argumentName = argumentName.Substring(5);
                }

                argumentName = $"{fieldNameOfTypeContainerClass}.{argumentName}";
                results.Add(argumentName);
            }
        }

        return results;
    }
}
