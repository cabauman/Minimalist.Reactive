using Microsoft.CodeAnalysis;

namespace Minimalist.Reactive.SourceGenerator.Blueprints
{
    internal class FieldDatum
    {
        public string Name { get; set; }

        public ITypeSymbol Type { get; set; }
    }
}
