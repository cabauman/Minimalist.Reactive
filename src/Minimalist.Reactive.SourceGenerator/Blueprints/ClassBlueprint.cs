using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Minimalist.Reactive.SourceGenerator.OperatorData;
using System.Linq;

namespace Minimalist.Reactive.SourceGenerator.Blueprints;

internal class ClassBlueprint : OperatorResult
{
    public string ClassName { get; set; }

    public string NamespaceName { get; set; }

    public Accessibility AccessModifier { get; set; }

    public bool IsPartial { get; set; }

    //public IReadOnlyList<SyntaxKind> SyntaxTokens { get; set; }

    public ClassSpecifierBlueprint BaseClass { get; set; }

    public IReadOnlyList<InterfaceSpecifierBlueprint> Interfaces { get; set; }

    public IReadOnlyList<ConstructorBlueprint> ConstructorBlueprints { get; set; }

    public IReadOnlyList<FieldBlueprint> Fields { get; set; }

    public IReadOnlyList<PropertyBlueprint> Properties { get; set; }

    public IReadOnlyList<MethodBlueprint> Methods { get; set; }

    public IReadOnlyList<ClassBlueprint> Classes { get; set; }

    public string CreateSource()
    {
        var partialText = IsPartial ? "partial" : string.Empty;
        var accessModifier = AccessModifier.ToFriendlyString();
        var doesInherit = BaseClass != null || Interfaces.Count > 0;
        var inheritList = string.Join(", ", Interfaces.Select(x => x.ToString()).Prepend(BaseClass.ToString()));

        var result = $@"
{accessModifier} {partialText} {ClassName} {inheritList}
{{
    {string.Join("\n", Fields.Select(x => x.CreateSource()))}
    {string.Join("\n", ConstructorBlueprints.Select(x => x.CreateSource()))}
    {string.Join("\n", Properties.Select(x => x.CreateSource()))}
    {string.Join("\n", Methods.Select(x => x.CreateSource()))}
    {string.Join("\n", Classes.Select(x => x.CreateSource()))}
}}
";



        return result;
    }
}

internal abstract class ConstructorBlueprint
{
    public string Name { get; set; }

    public string Accessibility { get; set; }

    public IReadOnlyList<ObservableClassMethodParameterBlueprint> Parameters { get; set; }

    public abstract string CreateSource();
}

internal static class CustomObservableClass
{
    internal static class Fields
    {
        public const string Parent = "_parent";
        public const string Observer = "_observer";
        public const string IsUpstreamComplete = "_isUpstreamComplete";
        public const string IsDisposed = "_isDisposed";
        public const string Disposables = "_disposables";
    }

    internal class ConstructorBlueprint : Blueprints.ConstructorBlueprint
    {
        private readonly IReadOnlyList<IOperatorLogic> _operatorLogic;

        public ConstructorBlueprint(IReadOnlyList<IOperatorLogic> operatorLogic)
        {
            _operatorLogic = operatorLogic;
        }

        public override string CreateSource()
        {
            return string.Empty;
        }
    }

    internal class MethodBlueprint : Blueprints.MethodBlueprint
    {
        private readonly IReadOnlyList<IOperatorLogic> _operatorLogic;

        public MethodBlueprint(IReadOnlyList<IOperatorLogic> operatorLogic)
        {
            _operatorLogic = operatorLogic;
        }

        public override string CreateSource()
        {
            return string.Empty;
        }
    }
}

internal static class CustomSubscriptionClass
{
    internal static class Fields
    {
        public const string Parent = "_parent";
        public const string Observer = "_observer";
        public const string IsUpstreamComplete = "_isUpstreamComplete";
        public const string IsDisposed = "_isDisposed";
        public const string Disposables = "_disposables";
    }

    internal class ConstructorBlueprint : Blueprints.ConstructorBlueprint
    {
        private readonly IReadOnlyList<IOperatorLogic> _operatorLogic;

        public ConstructorBlueprint(IReadOnlyList<IOperatorLogic> operatorLogic)
        {
            _operatorLogic = operatorLogic;
        }

        public override string CreateSource()
        {
            return string.Empty;
        }
    }

    internal class MethodBlueprint : Blueprints.MethodBlueprint
    {
        private readonly IReadOnlyList<IOperatorLogic> _operatorLogic;

        public MethodBlueprint(IReadOnlyList<IOperatorLogic> operatorLogic)
        {
            _operatorLogic = operatorLogic;
        }

        public override string CreateSource()
        {
            return string.Empty;
        }
    }
}

internal abstract class MethodBlueprint
{
    public string Name { get; set; }

    public string Accessibility { get; set; }

    public string ReturnType { get; set; }

    public IReadOnlyList<ObservableClassMethodParameterBlueprint> Parameters { get; set; }

    public abstract string CreateSource();
}

internal class PropertyBlueprint
{
    public string Name { get; set; }

    public string BackingFieldName { get; set; }

    public string InstanceTypeName { get; set; }

    public ITypeSymbol ReturnType { get; set; }

    public Accessibility Accessibility { get; set; }

    //public IImplementationLogic ImplementationLogic { get; set; }

    public string CreateSource()
    {
        var propertySource = @$"
private {ReturnType.ToDisplayString()} {BackingFieldName};
{Accessibility.ToFriendlyString()} {ReturnType.ToDisplayString()} {Name}
{{
    get
    {{
        if ({BackingFieldName} == null)
        {{
            {BackingFieldName} = new {InstanceTypeName}(this);
        }}
        return {BackingFieldName};
    }}
}}
";
        return propertySource;
    }

    public string CreateSource2()
    {
        var propertySource = @$"
{Accessibility.ToFriendlyString()} {ReturnType.ToDisplayString()} {Name} {{ get; }} = new {InstanceTypeName}();
";
        return propertySource;
    }
}

internal class FieldBlueprint
{
    public string Name { get; set; }

    public ITypeSymbol TypeSymbol { get; set; }

    public Accessibility Accessibility { get; set; }

    public string CreateSource()
    {
        return string.Empty;
    }
}

internal class ParameterBlueprint
{
    public string Name { get; set; }

    public ITypeSymbol TypeSymbol { get; set; }

    public string CreateSource()
    {
        return string.Empty;
    }
}

internal class InterfaceSpecifierBlueprint
{
    public string Name { get; set; }

    IReadOnlyList<ITypeSymbol> GenericTypeArguments { get; set; }
}

internal class ClassSpecifierBlueprint
{
    public string Name { get; set; }

    IReadOnlyList<ITypeSymbol> GenericTypeArguments { get; set; }
}
