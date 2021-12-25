using Minimalist.Reactive.SourceGenerator.Blueprints;

namespace Minimalist.Reactive.SourceGenerator.SourceCreator
{
    internal class RoslynSourceCreator : ISourceCreator
    {
        public string Create(TargetClassBlueprint classDatum)
        {
            foreach (var property in classDatum.Properties)
            {
            }
            return string.Empty;
        }
    }
}
