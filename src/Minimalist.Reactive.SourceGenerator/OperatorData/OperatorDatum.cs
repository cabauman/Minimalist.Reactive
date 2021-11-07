using Minimalist.Reactive.SourceGenerator.SourceCreator;

namespace Minimalist.Reactive.SourceGenerator.OperatorData
{
    internal interface IOperatorDatum
    {
        string Name { get; }

        bool RequiresScheduling { get; }

        IReadOnlyList<ArgDatum> ArgData { get; }

        IReadOnlyList<FieldDatum> Fields { get; }

        OperatorResult GetSource(RxSourceCreatorContext context);
    }
}
