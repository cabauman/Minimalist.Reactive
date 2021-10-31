using Microsoft.CodeAnalysis.CSharp.Syntax;
using Minimalist.Reactive.SourceGenerator.OperatorData;
using System.Text;

namespace Minimalist.Reactive.SourceGenerator.SourceCreator
{
    internal class StringBuilderSourceCreator : ISourceCreator
    {
        public string Create(ClassDatum classDatum)
        {
            var source = $@"
using System;
using Minimalist.Reactive.Linq;
using Minimalist.Reactive.Disposables;
namespace {classDatum.NamespaceName}
{{
    public partial class {classDatum.ClassName}
    {{
        private bool isUpstreamComplete;
        private IDisposable subscription;
        {string.Join("\n\n", classDatum.ClassContents.Select(ProcessEntry))}
    }}
}}
";

            return source;
        }

        private string ProcessEntry(ClassContent classContent)
        {
            return $@"
{ProcessObservableProperty(classContent.PropertyDatum)}
{ProcessObservableClass(classContent.ClassDatum)}
";
        }

        private string ProcessObservableProperty(ObservablePropertyDatum observablePropertyDatum)
        {
            var accessModifier = observablePropertyDatum.Accessibility.ToFriendlyString();
            var returnType = observablePropertyDatum.GenericType.ToDisplayString();
            var propertyName = observablePropertyDatum.Name;
            var methodName = observablePropertyDatum.OriginalMethodName;
            var propertySource = $"{accessModifier} {returnType} {propertyName} {{ get; }} = new {methodName}Observable();";
            return propertySource;
        }

        public string ProcessObservableClass(NestedClassDatum observableClassDatum)
        {
            var returnType = observableClassDatum.GenericType;
            var source = $@"
    private class {observableClassDatum.ClassName} : IObservable<{returnType}>
    {{
        {string.Join("\n\n", observableClassDatum.Methods.Select(x => ProcessMethod(x, observableClassDatum.Methods.Length)))}
    }}
";

            // Maybe rename ClassContents to RxifyRequests.
            //foreach (var method in observableClassDatum.Methods)
            //{
            //    var name = method.Name;
            //    var operatorData = method.OperatorData;
            //    var operatorFields = new List<FieldDatum>();
            //    foreach (var operatorDatum in operatorData)
            //    {
            //        OperatorResult result = operatorDatum.Name switch
            //        {
            //            "Return" => Return(operatorDatum),
            //            "Where" => Where(operatorDatum),
            //            "Select" => Select(operatorDatum),
            //            _ => throw new NotSupportedException(),
            //        };
            //        operatorFields.AddRange(result.Fields);
            //        var operatorSource = result.Source;
            //    }
            //}

            return source;
        }

        private string ProcessMethod(MethodDatum methodDatum, int methodCount)
        {
            var returnStatement = "return Disposable.Empty;";
            if (methodCount > 1)
            {
                returnStatement = @"
this.subscription = new BooleanDisposable();
return this.subscription;
";
            }

            return $@"
private {methodDatum.ReturnType} {methodDatum.Name}({methodDatum.ParameterType} {methodDatum.ParameterName})
{{
    {ProcessMethodContents(methodDatum)}
    {returnStatement}
}}
";
        }

        private string ProcessMethodContents(MethodDatum methodDatum)
        {
            var operatorData = methodDatum.OperatorData;
            var sb = new StringBuilder();
            var isWithinSubscribeMethod = methodDatum.Name == "Subscribe";
            var isInLoop = false;
            var localVarCounter = 0;
            foreach (var operatorDatum in operatorData)
            {
                OperatorResult result = operatorDatum.Name switch
                {
                    "Return" => Return(operatorDatum, localVarCounter),
                    "Where" => Where(operatorDatum, isWithinSubscribeMethod, isInLoop, localVarCounter),
                    "Select" => Select(operatorDatum, ++localVarCounter),
                    "OnNext" => ObserverOnNext(localVarCounter),
                    _ => throw new NotSupportedException(),
                };
                sb.AppendLine(result.Source);
            }
            return sb.ToString();
        }

        public OperatorResult Return(OperatorDatum operatorData, int localVarCounter)
        {
            var value = operatorData.ArgData[0].Expression.ToString();
            return new OperatorResult
            {
                Source = $"var x{localVarCounter} = {value};",
            };
        }

        public OperatorResult Where(OperatorDatum operatorData, bool isWithinSubscribeMethod, bool isInLoop, int localVarCounter)
        {
            var predicate = "";
            if (operatorData.ArgData[0].Expression is LambdaExpressionSyntax lambda)
            {
                predicate = lambda.Body.ToString();
            }

            var skipOnNextStatement = "return;";
            if (isWithinSubscribeMethod)
            {
                if (isInLoop)
                {
                    skipOnNextStatement = "continue;";
                }
                else
                {
                    skipOnNextStatement = "return Disposable.Empty;";
                }
            }

            return new OperatorResult
            {
                Source = @$"
if (!({predicate}))
{{
    if (isUpstreamComplete)
    {{
        observer.OnCompleted();
    }}
    {skipOnNextStatement}
}}
",
            };
        }

        public OperatorResult Select(OperatorDatum operatorData, int localVarCounter)
        {
            var selector = operatorData.ArgData[0].Expression.ToString();
            return new OperatorResult
            {
                Source = $"var x{localVarCounter} = {selector}(x{localVarCounter - 1});",
            };
        }

        private OperatorResult ObserverOnNext(int localVarCounter)
        {
            return new OperatorResult
            {
                Source = $@"
observer.OnNext(x{localVarCounter});
if (isUpstreamComplete)
{{
    observer.OnCompleted();
}}
",
            };
        }
    }
}
