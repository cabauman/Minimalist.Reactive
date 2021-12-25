namespace Minimalist.Reactive.SourceGenerator.SourceCreator
{
    internal class RxSourceCreatorContext
    {
        public bool IsWithinSubscribeMethod {  get; set; }

        public bool IsInLoop { get; set; }

        public int LocalVarCounter { get; set; }
    }
}
