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
                // TODO: this only works for static references. If it's a member of parent,
                // we need to do _parent.{expression}.
                _schedulerArgName = argData[1].Expression.ToString();
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
                    // TODO: Consider adding this to a collection of disposables in case of cancellation.
                    Source = $"{_schedulerArgName}.ScheduleAction(this, static @this => @this.Tick{0}({value}));",
                };
            }
            return new OperatorResult
            {
                Source = $"var x{localVarCounter} = {value};",
            };
        }
    }
}
