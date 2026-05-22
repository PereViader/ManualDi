namespace ManualDi.Async
{
    internal sealed class BindingChainNode
    {
        public readonly Binding Binding;
        public BindingChainNode? Next;

        public BindingChainNode(Binding binding)
        {
            Binding = binding;
        }
    }
}
