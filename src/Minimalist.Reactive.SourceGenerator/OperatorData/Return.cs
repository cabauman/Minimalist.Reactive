using Minimalist.Reactive.SourceGenerator.SourceCreator;

namespace Minimalist.Reactive.SourceGenerator.OperatorData
{
    internal class Return : IOperatorData
    {
        public string Accept(ISourceCreator sourceCreator)
        {
            return sourceCreator.Create(this);
        }
    }
}
