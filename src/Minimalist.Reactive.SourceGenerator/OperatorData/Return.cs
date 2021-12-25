using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.SourceCreator;

namespace Minimalist.Reactive.SourceGenerator.OperatorData
{
    internal class Return : IOperatorDatum
    {
        private readonly string? _schedulerArgName;

        public Return(List<ArgDatum> argData)
        {
            ArgData = argData;
            if (argData.Count > 1 && argData[1].Type.Name == "IScheduler")
            {
                _schedulerArgName = argData[1].Expression.ToString();
                if (argData[1].IsMemberOfTargetClass)
                {
                    if (_schedulerArgName.StartsWith("this."))
                    {
                        _schedulerArgName = _schedulerArgName.Substring(5);
                    }
                    _schedulerArgName = $"_parent.{_schedulerArgName}";
                }
            }

            Fields = Array.Empty<FieldDatum>();
        }

        public string Name => "Return";

        public bool RequiresScheduling => _schedulerArgName != null;

        public IReadOnlyList<ArgDatum> ArgData { get; }

        public IReadOnlyList<FieldDatum> Fields { get; }

        public OperatorResult GetSource(RxSourceCreatorContext context)
        {
            int localVarCounter = context.LocalVarCounter;
            var value = ArgData[0].Expression.ToString();
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
_isUpstreamComplete = true;
var x{localVarCounter} = {value};
",
            };
        }
    }
}
