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
        public static TypeBinding<TInterface, TConcrete> Dispose<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            InstanceContainerDelegate<TConcrete> disposeDelegate
            )
        {
            typeBinding.Inject((o, c) => c.QueueDispose(() => disposeDelegate.Invoke(o, c)));
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeTypeBinding Dispose(
            this UnsafeTypeBinding typeBinding,
            InstanceContainerDelegate<object> disposeDelegate
            )
        {
            typeBinding.Inject((o, c) => c.QueueDispose(() => disposeDelegate.Invoke(o, c)));
            return typeBinding;
        }
    }
}
