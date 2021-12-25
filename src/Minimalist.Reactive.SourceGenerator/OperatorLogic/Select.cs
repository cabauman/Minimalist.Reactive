using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.SourceCreator;

namespace Minimalist.Reactive.SourceGenerator.OperatorData
{
    internal class Select : IOperatorLogic
    {
        private readonly IReadOnlyList<OperatorArgument> _argData;

        public Select(List<OperatorArgument> argData)
        {
            _argData = argData;
        }

        public bool RequiresScheduling => false;

        public string GenericTypeArgument => throw new NotImplementedException();

        public IReadOnlyList<ObservableClassFieldBlueprint> Fields => Array.Empty<ObservableClassFieldBlueprint>();

        public OperatorResult GetSource(RxSourceCreatorContext context)
        {
            int localVarCounter = context.LocalVarCounter;
            var selector = _argData[0].Expression.ToString();
            return new OperatorResult
            {
                Source = $"var x{localVarCounter} = {selector}(x{localVarCounter - 1});",
            };
        }
    }
}
