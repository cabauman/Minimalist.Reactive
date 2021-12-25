using Microsoft.CodeAnalysis.CSharp.Syntax;
using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.SourceCreator;
using System.Text.RegularExpressions;

namespace Minimalist.Reactive.SourceGenerator.OperatorData;

internal class Where : IOperatorLogic
{
    private readonly IReadOnlyList<OperatorArgument> _argData;

    public Where(List<OperatorArgument> argData)
    {
        _argData = argData;
    }

    public bool RequiresScheduling => false;

    public string GenericTypeArgument => throw new NotImplementedException();

    public IReadOnlyList<ObservableClassFieldBlueprint> Fields => Array.Empty<ObservableClassFieldBlueprint>();

    public OperatorResult GetSource(RxSourceCreatorContext context)
    {
        bool isWithinSubscribeMethod = context.IsWithinSubscribeMethod;
        bool isInLoop = context.IsInLoop;
        var predicate = "";
        var lambdaParam = "";
        // TODO: Handle method group.
        // TODO: Move all this lambda handling logic to testable helper method.
        if (_argData[0].Expression is LambdaExpressionSyntax lambda)
        {
            predicate = lambda.Body.ToString();
            if (lambda is SimpleLambdaExpressionSyntax simpleLambda)
            {
                lambdaParam = simpleLambda.Parameter.ToString();
            }
            else if (lambda is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
            {
                // TODO: Handle multiple parameters.
                lambdaParam = parenthesizedLambda.ParameterList.Parameters[0].ToString();
            }
        }

        // TODO: Extract regex logic to testable helper method.
        var localVarName = $"x{context.LocalVarCounter}";
        var regex = new Regex($"(?<=[^a-zA-Z_@]|^){lambdaParam}(?=[^a-zA-Z_0-9]|$)");
        predicate = regex.Replace(predicate, localVarName, 1);
        //predicate.Replace(lambdaParam)

        var skipOnNextStatement = "return;";
        if (isWithinSubscribeMethod)
        {
            if (isInLoop)
            {
                skipOnNextStatement = "continue;";
            }
            else
            {
                skipOnNextStatement = "return Disposable.Empty;";
            }
        }

        var observerVarName = "_observer";
        if (context.IsWithinSubscribeMethod)
        {
            observerVarName = "observer";
        }

        return new OperatorResult
        {
            Source = @$"
if (!({predicate}))
{{
    if ({context.IsUpstreamCompleteFieldName})
    {{
        {observerVarName}.OnCompleted();
    }}
    {skipOnNextStatement}
}}
",
        };
    }
}
