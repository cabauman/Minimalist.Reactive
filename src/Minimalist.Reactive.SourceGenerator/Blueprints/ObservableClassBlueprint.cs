namespace Minimalist.Reactive.SourceGenerator.Blueprints;

// ClassBlueprint
internal class ObservableClassBlueprint
{
    public string ClassName { get; set; }

    public string GenericTypeArgument { get; set; }

    // Determined by the operators that depend on state.
    public IReadOnlyList<ObservableClassFieldBlueprint> Fields { get; set; }

    public IReadOnlyList<ObservableClassMethodBlueprint> Methods { get; set; }
}
