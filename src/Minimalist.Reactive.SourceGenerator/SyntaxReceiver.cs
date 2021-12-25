using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Minimalist.Reactive.SourceGenerator;

internal class SyntaxReceiver2 : ISyntaxContextReceiver
{
    public List<RxifyInput> Candidates { get; } = new();

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
                Candidates.Add(new RxifyInput() { Symbol = methodSymbol, Syntax = methodDeclarationSyntax });
            }
        }
    }
}
