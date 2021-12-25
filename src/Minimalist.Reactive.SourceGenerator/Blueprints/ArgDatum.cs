using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Minimalist.Reactive.SourceGenerator.Blueprints
{
    internal class ArgDatum
    {
        public string ParameterName { get; set; }

        public ITypeSymbol Type { get; set; }

        public ExpressionSyntax Expression { get; set; }

        public bool IsMemberOfTargetClass { get; set; }
    }
}
