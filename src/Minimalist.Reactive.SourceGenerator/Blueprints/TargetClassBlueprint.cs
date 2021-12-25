namespace Minimalist.Reactive.SourceGenerator.Blueprints;

internal partial class TargetClassBlueprint
{
    public string NamespaceName { get; set; }

    public string ClassName { get; set; }

    public IReadOnlyList<TargetClassComponentBlueprint> Components { get; set; }

    public IReadOnlyList<TargetClassPropertyBlueprint> Properties { get; set; }

    public IReadOnlyList<ObservableClassBlueprint> Classes { get; set; }
}
