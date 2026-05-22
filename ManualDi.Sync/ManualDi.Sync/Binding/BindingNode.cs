namespace ManualDi.Sync
{
    internal readonly struct BindingNode
    {
        public readonly Binding Binding;
        public readonly BindingChainNode? Next;

        public BindingNode(Binding binding, BindingChainNode? next = null)
        {
            Binding = binding;
            Next = next;
        }
    }
}
