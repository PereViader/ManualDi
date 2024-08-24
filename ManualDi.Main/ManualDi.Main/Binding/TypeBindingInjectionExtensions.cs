using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingInjectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> Inject<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding, 
            InjectionDelegate<TConcrete> injectionDelegate)
        {
            typeBinding.InjectionDelegates += injectionDelegate;
            return typeBinding;
        }
    }
}
