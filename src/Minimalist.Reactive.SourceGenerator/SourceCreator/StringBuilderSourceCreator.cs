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
{ProcessNestedObservableClass(classContent.ClassDatum, className)}
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

        public string ProcessNestedObservableClass(NestedClassDatum observableClassDatum, string parentClassName)
        {
            var returnType = observableClassDatum.GenericType;
            var source = $@"
    private class {observableClassDatum.ClassName} : IObservable<{returnType}>
    {{
        // Only needed if there are member references.
        private readonly {parentClassName} _parent = null;

        public {observableClassDatum.ClassName}({parentClassName} parent)
        {{
            _parent = parent;
        }}

        public IDisposable Subscribe(IObserver<{returnType}> observer)
        {{
            return new Subscription(observer, _parent);
            //return Disposable.Empty;
        }}

        // Only needed if included operator logic stores state.
        // Not needed for hot observables.
        public class Subscription : IDisposable
        {{
            private readonly {parentClassName} _parent = null;
            private IObserver<{returnType}> _observer = null;
            private bool _isUpstreamComplete = false;
            // TODO: Only needed for hot generator.
            //private List<IDisposable> _subscriptions = new List<IDisposable>();

            public Subscription(IObserver<{returnType}> observer, {parentClassName} parent)
            {{
                _observer = observer;
                _parent = parent;
            }}

            public void Run()
            {{
            }}

            public void Dispose()
            {{
                _isDisposed = true;
            }}

            {string.Join("\n\n", observableClassDatum.Methods.Select(x => ProcessMethod(x, observableClassDatum.Methods.Length)))}
        }}
    }}
";

            return source;
        }

        private string ProcessMethod(MethodDatum methodDatum, int methodCount)
        {
            var returnStatement = "";
            if (methodDatum.Name == "Subscribe")
            {
                returnStatement = @"
var subscription = new BooleanDisposable();
return subscription;
";
            }

            return $@"
{(methodDatum.Name == "Subscribe" ? "public" : "private")} {methodDatum.ReturnType} {methodDatum.Name}({methodDatum.ParameterType} {methodDatum.ParameterName})
{{
    {(methodDatum.Name == "Subscribe" && methodCount > 1 ? "_observer = observer;" : "")}
    {ProcessMethodContents(methodDatum)}
    {returnStatement}
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

    internal class RxSourceCreatorContext
    {
        public bool IsWithinSubscribeMethod {  get; set; }

        public bool IsInLoop { get; set; }

        public int LocalVarCounter { get; set; }
    }
}
