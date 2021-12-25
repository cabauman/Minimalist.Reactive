using Microsoft.CodeAnalysis;

namespace Minimalist.Reactive.SourceGenerator.Blueprints
{
    internal class ObservablePropertyDatum
    {
        public string Name { get; set; }

        //public string BackingFieldName { get; set; }

        // CustomObservableClassName
        public string OriginalMethodName { get; set; }

        // Might have to change this to be the whole type for special cases like IConnectableObservable.
        // Think about making this a string so we don't leave the "to string" logic up to the source creator.
        public ITypeSymbol GenericType { get; set; } // int for IObservable<int>

        // Think about making this a string so we don't leave the "to string" logic up to the source creator.
        public Accessibility Accessibility { get; set; }
    }
}
