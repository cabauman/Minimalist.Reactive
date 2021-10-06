using Minimalist.Reactive.SourceGenerator.OperatorData;

namespace Minimalist.Reactive.SourceGenerator.SourceCreator
{
    internal interface ISourceCreator
    {
        string Create(Where operatorData);
        string Create(Return operatorData);
    }
}
