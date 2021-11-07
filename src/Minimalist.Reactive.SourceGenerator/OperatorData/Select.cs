using Minimalist.Reactive.SourceGenerator.SourceCreator;

namespace Minimalist.Reactive.SourceGenerator.OperatorData
{
    internal class Select : IOperatorDatum
    {
        public Select(List<ArgDatum> argData)
        {
            ArgData = argData;
        }

        public string Name { get; }

        public bool RequiresScheduling => false;

        public IReadOnlyList<ArgDatum> ArgData { get; }

        public IReadOnlyList<FieldDatum> Fields => Array.Empty<FieldDatum>();

        public OperatorResult GetSource(RxSourceCreatorContext context)
        {
            int localVarCounter = context.LocalVarCounter;
            var selector = ArgData[0].Expression.ToString();
            return new OperatorResult
            {
                Source = $"var x{localVarCounter} = {selector}(x{localVarCounter - 1});",
            };
        }
    }
}
