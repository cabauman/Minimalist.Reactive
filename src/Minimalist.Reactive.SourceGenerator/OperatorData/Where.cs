using Microsoft.CodeAnalysis.CSharp.Syntax;
using Minimalist.Reactive.SourceGenerator.SourceCreator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minimalist.Reactive.SourceGenerator.OperatorData
{
    internal class Where : IOperatorData
    {
        public bool DoesRequireScheduling { get; }

        public string PredicateOperation { get; }

        public LambdaExpressionSyntax Lambda { get; } // Body: either block or expressionBody

        public string Accept(ISourceCreator sourceCreator)
        {
            return sourceCreator.Create(this);
        }
    }
}
