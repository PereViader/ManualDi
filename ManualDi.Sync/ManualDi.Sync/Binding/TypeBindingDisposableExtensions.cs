using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class BindingDisposableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding DontDispose<TBinding>(this TBinding binding)
            where TBinding : Binding
        {
            binding.TryToDispose = false;
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> Dispose<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            InstanceContainerDelegate<TConcrete> disposeDelegate
            )
        {
            binding.Inject((o, c) => c.QueueDispose(() => disposeDelegate.Invoke(o, c)));
            return binding;
        }
    }
}
