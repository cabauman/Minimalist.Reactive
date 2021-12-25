using Minimalist.Reactive.SourceGenerator.OperatorData;

namespace Minimalist.Reactive.SourceGenerator.Blueprints
{
    // Maybe make this class generic with IOperatorLogic as the type param.
    // MethodBlueprint<TLogic>
    internal class ObservableClassMethodBlueprint
    {
        public string Name { get; set; }

        public string Accessibility { get; set; }

        public string ReturnType { get; set; }

        public IReadOnlyList<ObservableClassMethodParameterBlueprint> Parameters { get; set; }

        public List<IOperatorLogic> OperatorLogicItems { get; set; }
    }
}
