using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Minimalist.Reactive.SourceGenerator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<InvocationExpressionSyntax> Candidates { get; } = new();

        /// <inheritdoc />
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
                && methodDeclarationSyntax.AttributeLists.Count > 0)
            {
                //IFieldSymbol fieldSymbol = syntaxNode.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
                //if (fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "AutoNotify.AutoNotifyAttribute"))
                //{
                //    Fields.Add(fieldSymbol);
                //}
            }
        }
    }

    internal class Data
    {
        public IMethodSymbol Symbol { get; set; }

        public MethodDeclarationSyntax Syntax { get; set; }
    }

    internal class SyntaxReceiver2 : ISyntaxContextReceiver
    {
        public List<Data> Candidates { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is MethodDeclarationSyntax methodDeclarationSyntax
                && methodDeclarationSyntax.AttributeLists.Count > 0)
            {
                var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as IMethodSymbol;
                if (symbol is not IMethodSymbol methodSymbol)
                {
                    return;
                }

                // TODO: Check if return type is IObservable.
                if (methodSymbol.GetAttributes().Any(attributeData => attributeData.AttributeClass?.ToDisplayString() == "Minimalist.Reactive.RxifyAttribute"))
                {
                    Candidates.Add(new Data() { Symbol = methodSymbol, Syntax = methodDeclarationSyntax });
                }
            }
        }
    }
}
