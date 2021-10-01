using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Text;

namespace Minimalist.Reactive.SourceGenerator
{
    [Generator]
    public class RxSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            {
                return;
            }

            Compilation compilation = context.Compilation;

            ArgumentSyntax returnArg;
            ArgumentSyntax whereArg;
            ArgumentSyntax selectArg;
            foreach (var invocationExpression in syntaxReceiver.Candidates)
            {
                var model = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
                var symbol = model.GetSymbolInfo(invocationExpression).Symbol;

                if (symbol is not IMethodSymbol methodSymbol)
                {
                    continue;
                }

                if (methodSymbol.Name == "Return")
                {
                    returnArg = invocationExpression.ArgumentList.Arguments[0];
                }

                if (methodSymbol.Name == "Where")
                {
                    whereArg = invocationExpression.ArgumentList.Arguments[0];
                }

                if (methodSymbol.Name == "Select")
                {
                    selectArg = invocationExpression.ArgumentList.Arguments[0];
                }

                //foreach (var argument in invocationExpression.ArgumentList.Arguments)
                //{

                //}
            }

            var source = $@"
using System;
using Minimalist.Reactive.Linq;
public partial class MyCoolClass
{{
    public IObservable<string> DoSomethingProperty {{ get; }} = new MyObservable();

    private class MyObservable : IObservable<string>
    {{
        public IDisposable Subscribe(IObserver<string> observer)
        {{
            var x = 1;
            if (x > 0)
            {{
                var s = x.ToString();
                observer.OnNext(s);
            }}
            observer.OnCompleted();
        }}
    }}
}}";
            context.AddSource($"MyCoolClass.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }
}
