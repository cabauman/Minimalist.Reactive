using Minimalist.Reactive.SourceGenerator.Blueprints;

namespace Minimalist.Reactive.SourceGenerator.SourceCreator
{
    internal interface ISourceCreator
    {
        string Create(ClassDatum classDatum);
    }
}
