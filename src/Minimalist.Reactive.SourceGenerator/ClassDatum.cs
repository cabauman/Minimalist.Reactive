using Microsoft.CodeAnalysis;
using Minimalist.Reactive.SourceGenerator.OperatorData;

namespace Minimalist.Reactive.SourceGenerator
{
    internal partial class ClassDatum
    {
        public string NamespaceName { get; }

        public string ClassName { get; }

        //public NestedClassDatum[] NestedClassData { get; }

        // Or a readonly list of container objects.
        public Dictionary<ObservableProperty, NestedClassDatum> PropertyToClassMap { get; }
    }

    // ObservableClassDatum
    internal class NestedClassDatum
    {
        public string ClassName { get; }

        // Determined by the operators that depend on state.
        // Also store the Subscribe observer if necessary.
        // Maybe an isDisposed field too.
        public IOperatorData[] Fields { get; }

        public MethodToBeGenerated[] Methods { get; }
    }

    // ObservablePropertyDatum
    internal class ObservableProperty
    {
        public string Name { get; }

        public INamedTypeSymbol GenericType { get; } // int for IObservable<int>

        public Accessibility Accessibility { get; }
    }

    internal class MethodToBeGenerated
    {
        public string Name { get; }

        public IOperatorData[] OperatorData { get; }
    }
}
