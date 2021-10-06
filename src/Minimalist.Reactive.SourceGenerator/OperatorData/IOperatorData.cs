using Minimalist.Reactive.SourceGenerator.SourceCreator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minimalist.Reactive.SourceGenerator.OperatorData
{
    internal interface IOperatorData
    {
        string GetField();

        string Accept(ISourceCreator sourceCreator);
    }
}
