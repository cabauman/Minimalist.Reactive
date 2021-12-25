using Microsoft.CodeAnalysis;

namespace Minimalist.Reactive.SourceGenerator.Blueprints
{
    internal class TargetClassPropertyBlueprint
    {
        public string Name { get; set; }

        public string BackingFieldName { get; set; }

        // CustomObservableClassName
        public string OriginalMethodName { get; set; }

        public string InstanceTypeName { get; set; }

        // Think about making this a string so we don't leave the "to string" logic up to the source creator.
        public ITypeSymbol ReturnType { get; set; }

        // Think about making this a string so we don't leave the "to string" logic up to the source creator.
        public Accessibility Accessibility { get; set; }
    }
}
