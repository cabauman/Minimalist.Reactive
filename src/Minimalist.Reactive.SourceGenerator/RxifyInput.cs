using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Minimalist.Reactive.SourceGenerator
{
    internal class RxifyInput
    {
        public IMethodSymbol Symbol { get; set; }

        public MethodDeclarationSyntax Syntax { get; set; }
    }
}
