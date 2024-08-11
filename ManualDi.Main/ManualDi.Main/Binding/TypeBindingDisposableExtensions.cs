using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public delegate Action GetDisposeDelegate<in T>(T instance, IDiContainer container);

    public static class TypeBindingDisposableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> Dispose<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding
            )
        {
            typeBinding.TryToDispose = true;
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> DontDispose<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding
        )
        {
            typeBinding.TryToDispose = false;
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> Dispose<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            GetDisposeDelegate<TConcrete> getDisposeDelegate
            )
        {
            typeBinding.TryToDispose = false;
            typeBinding.Inject((o, c) => c.QueueDispose(getDisposeDelegate.Invoke(o, c)));
            return typeBinding;
        }
    }
}
