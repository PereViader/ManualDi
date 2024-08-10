using System;

namespace ManualDi.Main
{
    public delegate Action GetDisposeDelegate<in T>(T instance, IDiContainer container);

    public static class TypeBindingDisposableExtensions
    {
        public static TypeBinding<TInterface, TConcrete> Dispose<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding
            )
        {
            typeBinding.TryToDispose = true;
            return typeBinding;
        }

        public static TypeBinding<TInterface, TConcrete> DontDispose<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding
        )
        {
            typeBinding.TryToDispose = false;
            return typeBinding;
        }

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
