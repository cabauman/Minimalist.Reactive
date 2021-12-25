﻿using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.SourceCreator;

namespace Minimalist.Reactive.SourceGenerator.OperatorData;

internal class ObserverOnNext : IOperatorLogic
{
    public string Name { get; }

    public bool RequiresScheduling => false;

    public string GenericTypeArgument => throw new NotImplementedException();

    public IReadOnlyList<OperatorArgument> ArgData { get; }

    public IReadOnlyList<ObservableClassFieldBlueprint> Fields => Array.Empty<ObservableClassFieldBlueprint>();

    public OperatorResult GetSource(RxSourceCreatorContext context)
    {
        var observerVarName = "_observer";
        if (context.IsWithinSubscribeMethod)
        {
            observerVarName = "observer";
        }
        int localVarCounter = context.LocalVarCounter;
        return new OperatorResult
        {
            Source = $@"
{observerVarName}.OnNext(x{localVarCounter});
if (_isUpstreamComplete)
{{
    {observerVarName}.OnCompleted();
}}
",
        };
    }
}
