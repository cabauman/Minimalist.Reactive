using Microsoft.CodeAnalysis;

namespace Minimalist.Reactive.SourceGenerator.Blueprints
{
    // ClassField
    internal class ObservableClassFieldBlueprint
    {
        public string Name { get; set; }

        public ITypeSymbol Type { get; set; }
    }
}
