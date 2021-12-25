using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.SourceCreator;

namespace Minimalist.Reactive.SourceGenerator.OperatorData
{
    internal class ObserverOnNext : IOperatorDatum
    {
        public string Name { get; }

        public bool RequiresScheduling => false;

        public IReadOnlyList<ArgDatum> ArgData { get; }

        public IReadOnlyList<FieldDatum> Fields => Array.Empty<FieldDatum>();

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
}
