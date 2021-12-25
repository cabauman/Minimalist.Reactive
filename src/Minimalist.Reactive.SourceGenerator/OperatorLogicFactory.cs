using Minimalist.Reactive.SourceGenerator.Blueprints;
using Minimalist.Reactive.SourceGenerator.OperatorData;

namespace Minimalist.Reactive.SourceGenerator;

internal static class OperatorLogicFactory
{
    public static IOperatorLogic Create(string operatorName, string genericTypeArgument, List<OperatorArgument> arguments)
    {
        return operatorName switch
        {
            "Return" => new Return(genericTypeArgument, arguments),
            "Where" => new Where(arguments),
            "Select" => new Select(arguments),
            _ => throw new NotSupportedException(),
        };
    }
}
