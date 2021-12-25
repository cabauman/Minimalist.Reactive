using Minimalist.Reactive.SourceGenerator.OperatorData;

namespace Minimalist.Reactive.SourceGenerator.Blueprints
{
    internal class MethodDatum
    {
        public string Name { get; set; }

        public string Accessibility { get; set; }

        public string ReturnType { get; set; }

        public IReadOnlyList<ParameterDatum> ParameterData { get; set; }

        public List<IOperatorDatum> OperatorData { get; set; }
    }
}
