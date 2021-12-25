namespace Minimalist.Reactive.SourceGenerator.Blueprints
{
    internal partial class ClassDatum
    {
        public string NamespaceName { get; set; }

        public string ClassName { get; set; }

        public IReadOnlyList<ClassContent> ClassContents { get; set; }
    }
}
