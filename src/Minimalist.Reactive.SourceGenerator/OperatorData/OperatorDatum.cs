namespace Minimalist.Reactive.SourceGenerator.OperatorData
{
    internal class OperatorDatum
    {
        public OperatorDatum(string operatorName, IReadOnlyList<ArgDatum> argData)
        {
            Name = operatorName;
            ArgData = argData;
        }

        public string Name { get; }

        public IReadOnlyList<ArgDatum> ArgData { get; }

        //IReadOnlyList<FieldDatum> RequiredFields { get; }
    }
}
