namespace Minimalist.Reactive.SourceGenerator.Blueprints
{
    internal class CustomObservableClass
    {
        public string ClassName { get; set; }

        public string GenericType { get; set;}

        // Determined by the operators that depend on state.
        // Also store the Subscribe observer if necessary.
        // Maybe an isDisposed field too.
        public FieldDatum[] Fields { get; set; }

        public MethodDatum[] Methods { get; set; }
    }
}
