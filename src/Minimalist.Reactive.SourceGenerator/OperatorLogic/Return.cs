using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.SourceCreator;

namespace Minimalist.Reactive.SourceGenerator.OperatorData;

internal class Return : IOperatorLogic
{
    private readonly IReadOnlyList<OperatorArgument> _argData;
    private readonly string? _schedulerArgName;

    public Return(string genericTypeArgument, List<OperatorArgument> argData)
    {
        GenericTypeArgument = genericTypeArgument;
        _argData = argData;
        // TODO: Move this calculation to a utility method.
        if (argData.Count > 1 && argData[1].Type.Name == "IScheduler")
        {
            _schedulerArgName = argData[1].Expression.ToString();
            if (argData[1].DoesOriginateFromTargetClass)
            {
                if (_schedulerArgName.StartsWith("this."))
                {
                    _schedulerArgName = _schedulerArgName.Substring(5);
                }
                _schedulerArgName = $"_parent.{_schedulerArgName}";
            }
        }

        Fields = Array.Empty<ObservableClassFieldBlueprint>();
    }

    public bool RequiresScheduling => _schedulerArgName != null;

    public string GenericTypeArgument { get; }

    public IReadOnlyList<ObservableClassFieldBlueprint> Fields { get; }

    public OperatorResult GetSource(RxSourceCreatorContext context)
    {
        int localVarCounter = context.LocalVarCounter;
        var value = _argData[0].Expression.ToString();
        if (RequiresScheduling)
        {
            return new OperatorResult
            {
                Source = @$"
_isUpstreamComplete = true;
_disposables.Add({_schedulerArgName}.ScheduleAction(this, static @this => @this.Tick{0}({value})));
",
            };
        }
        return new OperatorResult
        {
            Source = @$"
{context.IsUpstreamCompleteFieldName} = true;
var x{localVarCounter} = {value};
",
        };
    }
}
