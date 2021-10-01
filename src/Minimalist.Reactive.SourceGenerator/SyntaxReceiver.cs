using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace Minimalist.Reactive.SourceGenerator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<InvocationExpressionSyntax> Candidates { get; } = new();

        /// <inheritdoc />
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif 

            if (syntaxNode is InvocationExpressionSyntax invocationExpression)
            {
                var methodName = (invocationExpression.Expression as MemberAccessExpressionSyntax)?.Name.ToString() ??
                    (invocationExpression.Expression as MemberBindingExpressionSyntax)?.Name.ToString();

                if (string.Equals(methodName, "Select") || string.Equals(methodName, "Where"))
                {
                    Candidates.Add(invocationExpression);
                }
            }
        }
    }
}
