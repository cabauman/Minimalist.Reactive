using Minimalist.Reactive.SourceGenerator.Blueprints;
using System.Text;

namespace Minimalist.Reactive.SourceGenerator.SourceCreator
{
    internal class StringBuilderSourceCreator : ISourceCreator
    {
        public string Create(ClassDatum classDatum)
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
        {string.Join("\n\n", classDatum.ClassContents.Select(x => ProcessClassContent(x, classDatum.ClassName)))}
    }}
}}
";

            return source;
        }

        private string ProcessClassContent(ClassContent classContent, string className)
        {
            return $@"
{ProcessObservableProperty(classContent.PropertyDatum)}
{ProcessCustomObservableClass(classContent.ClassDatum, className)}
";
        }

        private string ProcessObservableProperty(ObservablePropertyDatum observablePropertyDatum)
        {
            var accessModifier = observablePropertyDatum.Accessibility.ToFriendlyString();
            var returnType = observablePropertyDatum.GenericType.ToDisplayString();
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

        private string ProcessCustomObservableClass(CustomObservableClass observableClassDatum, string parentClassName)
        {
            var genericType = observableClassDatum.GenericType;
            var source = $@"
    private class {observableClassDatum.ClassName} : IObservable<{genericType}>
    {{
        // Only needed if there are member references.
        private readonly {parentClassName} _parent = null;
        // TODO: Only needed for hot generator.
        //private List<IDisposable> _subscriptions = new List<IDisposable>();

        public {observableClassDatum.ClassName}({parentClassName} parent)
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
            private IObserver<{genericType}> _observer = null;
            private bool _isUpstreamComplete = false;
            private bool _isDisposed = false;
            private List<IDisposable> _disposables = new List<IDisposable>();

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

            {string.Join("\n\n", observableClassDatum.Methods.Select(x => ProcessMethod(x)))}
        }}
    }}
";

            return source;
        }

        private string ProcessMethod(MethodDatum methodDatum)
        {
            var parameters = string.Join(", ", methodDatum.ParameterData.Select(x => $"{x.Type} {x.Name}"));
            return $@"
{methodDatum.Accessibility} {methodDatum.ReturnType} {methodDatum.Name}({parameters})
{{
    // Remove this case for Run()
    if (_isDisposed)
    {{
        return;
    }}
    
    {ProcessMethodContents(methodDatum)}
}}
";
        }

        private string ProcessMethodContents(MethodDatum methodDatum)
        {
            var operatorData = methodDatum.OperatorData;
            var sb = new StringBuilder();
            var context = new RxSourceCreatorContext()
            {
                LocalVarCounter = 0,
                // TODO: Determine if we're in a loop.
                IsInLoop = false,
                IsWithinSubscribeMethod = methodDatum.Name == "Subscribe",
            };
            foreach (var operatorDatum in operatorData)
            {
                OperatorResult result = operatorDatum.GetSource(context);
                sb.AppendLine(result.Source);
            }
            return sb.ToString();
        }
    }
}
