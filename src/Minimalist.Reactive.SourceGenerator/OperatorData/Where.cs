using Microsoft.CodeAnalysis.CSharp.Syntax;
using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.SourceCreator;
using System.Text.RegularExpressions;

namespace Minimalist.Reactive.SourceGenerator.OperatorData
{
    internal class Where : IOperatorDatum
    {
        public Where(List<ArgDatum> argData)
        {
            ArgData = argData;
        }

        public string Name { get; }

        public bool RequiresScheduling => false;

        public IReadOnlyList<ArgDatum> ArgData { get; }

        public IReadOnlyList<FieldDatum> Fields => Array.Empty<FieldDatum>();

        public OperatorResult GetSource(RxSourceCreatorContext context)
        {
            bool isWithinSubscribeMethod = context.IsWithinSubscribeMethod;
            bool isInLoop = context.IsInLoop;
            var predicate = "";
            var lambdaParam = "";
            if (ArgData[0].Expression is LambdaExpressionSyntax lambda)
            {
                predicate = lambda.Body.ToString();
                if (lambda is SimpleLambdaExpressionSyntax simpleLambda)
                {
                    lambdaParam = simpleLambda.Parameter.ToString();
                }
            }

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
    if (_isUpstreamComplete)
    {{
        {observerVarName}.OnCompleted();
    }}
    {skipOnNextStatement}
}}
",
            };
        }
    }
}
