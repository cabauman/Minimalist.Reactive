﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Minimalist.Reactive.SourceGenerator.OperatorData;

namespace Minimalist.Reactive.SourceGenerator
{
    internal partial class ClassDatum
    {
        public string NamespaceName { get; set; }

        public string ClassName { get; set; }

        public IReadOnlyList<ClassContent> ClassContents { get; set; }
    }

    internal class ClassContent
    {
        public ObservablePropertyDatum PropertyDatum { get; set; }

        public NestedClassDatum ClassDatum { get; set; }
    }

    // ObservableClassDatum
    internal class NestedClassDatum
    {
        public string ClassName { get; set; }

        public string GenericType { get; set;}

        // Determined by the operators that depend on state.
        // Also store the Subscribe observer if necessary.
        // Maybe an isDisposed field too.
        public FieldDatum[] Fields { get; set; }

        public MethodDatum[] Methods { get; set; }
    }

    internal class ObservablePropertyDatum
    {
        public string Name { get; set; }

        public string OriginalMethodName { get; set; }

        // Might have to change this to be the whole type for special cases like IConnectableObservable.
        public ITypeSymbol GenericType { get; set; } // int for IObservable<int>

        public Accessibility Accessibility { get; set; }
    }

    internal class MethodDatum
    {
        public string Name { get; set; }

        public string ReturnType { get; set; }

        public string ParameterName { get; set; }

        public string ParameterType { get; set; }

        public List<IOperatorDatum> OperatorData { get; set; }
    }

    internal class ArgDatum
    {
        public string ParameterName { get; set; }

        public ITypeSymbol Type { get; set; }

        public ExpressionSyntax Expression { get; set; }
    }

    internal class FieldDatum
    {
        public string Name { get; set; }

        public ITypeSymbol Type { get; set; }
    }

    internal class OperatorResult
    {
        public string Source { get; set; }
    }
}
