using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class BindingDisposableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding SkipDisposable<TBinding>(this TBinding binding)
            where TBinding : Binding
        {
            binding.TryToDispose = false;
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Dispose<TBinding>(
            this TBinding binding,
            InstanceContainerDelegate disposeDelegate
            )
            where TBinding : Binding
        {
            binding.Inject((o, c) => c.QueueDispose(() => disposeDelegate.Invoke(o, c)));
            return binding;
        }
    }
}
