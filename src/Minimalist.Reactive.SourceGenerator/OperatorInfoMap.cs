using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.OperatorData;

namespace Minimalist.Reactive.SourceGenerator
{
    internal static class OperatorInfoMap
    {
        public static IOperatorDatum GetInfo(string operatorName, List<ArgDatum> argData)
        {
            return operatorName switch
            {
                "Return" => new Return(argData),
                "Where" => new Where(argData),
                "Select" => new Select(argData),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
