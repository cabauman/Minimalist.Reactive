using Minimalist.Reactive.SourceGenerator.Blueprints;
using System.Text;

namespace Minimalist.Reactive.SourceGenerator.SourceCreator;

internal class StringBuilderSourceCreator : ISourceCreator
{
    public string Create(TargetClassBlueprint classDatum)
    {
        var source = $@"
using System;
using System.Collections.Generic;
using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Linq;
using Minimalist.Reactive.Disposables;
namespace {classDatum.NamespaceName}
{{
    public partial class {classDatum.ClassName}
    {{
        {string.Join("\n\n", classDatum.Components.Select(x => ProcessClassContent(x, classDatum.ClassName)))}
    }}
}}
";

        return source;
    }

    private string ProcessClassContent(TargetClassComponentBlueprint classContent, string className)
    {
        return $@"
{ProcessObservableProperty(classContent.PropertyDatum)}
{ProcessCustomObservableClass(classContent.ClassDatum, className)}
";
    }

    private string ProcessObservableProperty(TargetClassPropertyBlueprint observablePropertyDatum)
    {
        var accessModifier = observablePropertyDatum.Accessibility.ToFriendlyString();
        var returnType = observablePropertyDatum.ReturnType.ToDisplayString();
        var propertyName = observablePropertyDatum.Name;
        var methodName = observablePropertyDatum.OriginalMethodName;
        var propertySource = @$"
private {returnType} _{propertyName};
{accessModifier} {returnType} {propertyName}
{{
    get
    {{
        if (_{propertyName} == null)
        {{
            _{propertyName} = new {methodName}Observable(this);
        }}
        return _{propertyName};
    }}
}}
";
        return propertySource;
    }

    private string ProcessCustomObservableClass(ObservableClassBlueprint customObservableClass, string parentClassName)
    {
        var genericType = customObservableClass.GenericTypeArgument;
        var source = $@"
    private class {customObservableClass.ClassName} : IObservable<{genericType}>
    {{
        // Only needed if there are member references.
        private readonly {parentClassName} _parent = null;
        // TODO: Only needed for hot generator.
        //private List<IDisposable> _subscriptions = new List<IDisposable>();

        public {customObservableClass.ClassName}({parentClassName} parent)
        {{
            _parent = parent;
        }}

        public IDisposable Subscribe(IObserver<{genericType}> observer)
        {{
            var subscription = new Subscription(observer, _parent);
            subscription.Run();
            return subscription;
        }}

        // Only needed if included operator logic stores state.
        // Not needed for hot observables.
        public class Subscription : IDisposable
        {{
            private readonly {parentClassName} _parent = null;
            private readonly IObserver<{genericType}> _observer = null;
            private bool _isUpstreamComplete = false;
            private bool _isDisposed = false;
            private readonly List<IDisposable> _disposables = new List<IDisposable>();

            public Subscription(IObserver<{genericType}> observer, {parentClassName} parent)
            {{
                _observer = observer;
                _parent = parent;
            }}

            public void Dispose()
            {{
                _isDisposed = true;
                foreach (var d in _disposables)
                {{
                    d.Dispose();
                }}
            }}

            {string.Join("\n\n", customObservableClass.Methods.Select(x => ProcessMethod(x)))}
        }}
    }}
";

        return source;
    }

    private string ProcessMethod(ObservableClassMethodBlueprint methodDatum)
    {
        var parameters = string.Join(", ", methodDatum.Parameters.Select(x => $"{x.Type} {x.Name}"));

        var disposeCheck = string.Empty;
        if (methodDatum.Name != "Run")
        {
            disposeCheck = @"
if (_isDisposed)
{{
    return;
}}
";
        }
        
        return $@"
{methodDatum.Accessibility} {methodDatum.ReturnType} {methodDatum.Name}({parameters})
{{
    {disposeCheck}
    {ProcessMethodContents(methodDatum)}
}}
";
    }

    private string ProcessMethodContents(ObservableClassMethodBlueprint methodDatum)
    {
        var operatorData = methodDatum.OperatorLogicItems;
        var sb = new StringBuilder();
        var context = new RxSourceCreatorContext
        {
            LocalVarCounter = 0,
            // TODO: Determine if we're in a loop.
            IsInLoop = false,
            IsWithinSubscribeMethod = methodDatum.Name == "Subscribe",
            IsUpstreamCompleteFieldName = "_isUpstreamComplete",
        };
        foreach (var operatorDatum in operatorData)
        {
            OperatorResult result = operatorDatum.GetSource(context);
            sb.AppendLine(result.Source);
        }
        return sb.ToString();
    }
}
