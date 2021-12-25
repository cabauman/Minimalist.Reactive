using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.SourceCreator;

namespace Minimalist.Reactive.SourceGenerator.OperatorData;

internal interface IOperatorLogic
{
    bool RequiresScheduling { get; }

    string GenericTypeArgument { get; }

    IReadOnlyList<ObservableClassFieldBlueprint> Fields { get; }

    OperatorResult GetSource(RxSourceCreatorContext context);
}
