using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingDisposableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding DontDispose<TBinding>(this TBinding typeBinding)
            where TBinding : TypeBinding
        {
            typeBinding.TryToDispose = false;
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TApparent, TConcrete> Dispose<TApparent, TConcrete>(
            this TypeBinding<TApparent, TConcrete> typeBinding,
            InstanceContainerDelegate<TConcrete> disposeDelegate
            )
        {
            typeBinding.Inject((o, c) => c.QueueDispose(() => disposeDelegate.Invoke(o, c)));
            return typeBinding;
        }
    }
}
