using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.OperatorData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minimalist.Reactive.SourceGenerator;

public class SemanticModelProvider
{
    private readonly Compilation _compilation;

    public SemanticModelProvider(Compilation compilation)
    {
        _compilation = compilation;
    }

    public SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
    {
        return _compilation.GetSemanticModel(syntaxTree);
    }
}

internal class Utils
{
    public static List<InvocationExpressionSyntax> ExtractInvocationExpressions(MethodDeclarationSyntax methodSyntax)
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

    public static List<OperatorArgument> ExtractOperatorArguments(
        IReadOnlyList<IParameterSymbol> parameterSymbols,
        IReadOnlyList<ArgumentSyntax> argumentSyntaxList)
    {
        var operatorArgumentList = new List<OperatorArgument>();
        for (int i = 0; i < argumentSyntaxList.Count; i++)
        {
            var operatorArgument = new OperatorArgument
            {
                ParameterName = parameterSymbols[i].Name,
                Type = parameterSymbols[i].Type,
                Expression = argumentSyntaxList[i].Expression,
            };
            operatorArgumentList.Add(operatorArgument);
        }

        return operatorArgumentList;
    }
}

internal class OperatorLogicExtractor
{
    private readonly SemanticModelProvider _semanticModelProvider;

    public OperatorLogicExtractor(SemanticModelProvider semanticModelProvider)
    {
        _semanticModelProvider = semanticModelProvider;
    }

    public IReadOnlyList<IOperatorLogic> Create(MethodDeclarationSyntax methodDeclarationSyntax)
    {
        var model = _semanticModelProvider.GetSemanticModel(methodDeclarationSyntax.SyntaxTree);
        var invocationExpressions = Utils.ExtractInvocationExpressions(methodDeclarationSyntax);
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
            var operatorArguments = Utils.ExtractOperatorArguments(parameterSymbols, argumentSyntaxItems);
            var genericTypeArgument = operatorMethodSymbol.TypeArguments[0].ToDisplayString();
            var operatorLogic = OperatorLogicFactory.Create(operatorMethodSymbol.Name, genericTypeArgument, operatorArguments);
            operatorLogicItems.Add(operatorLogic);
        }

        operatorLogicItems.Add(new ObserverOnNext());

        return operatorLogicItems;
    }
}

internal class ClassBlueprintCreator
{
    public TargetClassBlueprint Create(ExtractedClassDatum classDatum)
    {
        var classComponents = new List<TargetClassComponentBlueprint>();
        foreach (var methodDatum in classDatum.MethodData)
        {
            var classComponent = GetClassComponentBlueprint(methodDatum);
            classComponents.Add(classComponent);
        }

        var properties = new List<TargetClassPropertyBlueprint>(classComponents.Count);
        var nestedClasses = new List<ObservableClassBlueprint>(classComponents.Count);
        foreach (var classComponent in classComponents)
        {
            properties.Add(classComponent.PropertyDatum);
            nestedClasses.Add(classComponent.ClassDatum);
        }

        return new TargetClassBlueprint
        {
            NamespaceName = classDatum.NamespaceName,
            ClassName = classDatum.ClassName,
            //Fields = fields,
            Properties = properties,
            Classes = nestedClasses,
        };
    }

    private TargetClassComponentBlueprint GetClassComponentBlueprint(MethodDatum methodDatum)
    {
        var methodName = methodDatum.MethodName;
        var operatorLogicItems = methodDatum.OperatorLogicItems;
        var genericTypeArgument = methodDatum.ReturnType.TypeArguments[0];

        return new TargetClassComponentBlueprint
        {
            PropertyDatum = new TargetClassPropertyBlueprint
            {
                Name = $"{methodName}Property",
                // TODO: Handle OutOfRangeException
                BackingFieldName = $"_{char.ToLower(methodName[0]) + methodName.Substring(1)}",
                //ReturnType = methodDatum.ReturnTypeName,
                InstanceTypeName = $"{methodName}Observable",
                Accessibility = methodDatum.Accessibility,
            },
            ClassDatum = new ObservableClassBlueprint
            {
                ClassName = $"{methodName}Observable",
                GenericTypeArgument = genericTypeArgument.ToDisplayString(),
                //Fields = operatorFields.ToArray(),
                Methods = new MethodBlueprintCreator().Create(operatorLogicItems),
            },
        };
    }
}

internal class MethodBlueprintCreator
{
    public IReadOnlyList<ObservableClassMethodBlueprint> Create(IReadOnlyList<IOperatorLogic> operatorLogicItems)
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
}

internal class MethodDataExtractor
{
    private readonly SemanticModelProvider _semanticModelProvider;

    public void Extract(RxifyInput method)
    {

    }
}

internal class ExtractedClassDatum
{
    public string NamespaceName { get; set; }

    public string ClassName { get; set; }

    public IReadOnlyList<MethodDatum> MethodData { get; set; }
}

internal class MethodDatum
{
    public string MethodName { get; set; }

    public INamedTypeSymbol ReturnType { get; set; }

    public Accessibility Accessibility { get; set; }

    public IReadOnlyList<IOperatorLogic> OperatorLogicItems { get; set; }
}

internal class ClassDatumExtractor
{
    private readonly OperatorLogicExtractor _operatorLogicExtractor;

    public ClassDatumExtractor(OperatorLogicExtractor operatorLogicCreator)
    {
        _operatorLogicExtractor = operatorLogicCreator;
    }

    public ExtractedClassDatum? Extract(INamedTypeSymbol classSymbol, IReadOnlyList<RxifyInput> methods)
    {
        if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
        {
            // TODO: Issue a diagnostic that it must be top level.
            return null;
        }

        string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        string className = classSymbol.Name;

        var methodData = new List<MethodDatum>();
        foreach (var method in methods)
        {
            methodData.Add(ProcessMethod(method));
        }

        return new ExtractedClassDatum
        {
            NamespaceName = namespaceName,
            ClassName = className,
            MethodData = methodData,
        };
    }

    private MethodDatum ProcessMethod(RxifyInput method)
    {
        if (method.Symbol.ReturnType is not INamedTypeSymbol returnType)
        {
            return null;
        }

        var methodName = method.Symbol.Name;

        return new MethodDatum
        {
            MethodName = methodName,
            ReturnType = returnType,
            OperatorLogicItems = _operatorLogicExtractor.Create(method.Syntax),
        };
    }
}
