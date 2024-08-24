using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingInitializationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> Initialize<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            InitializationDelegate<TConcrete> initializationDelegate
            )
        {
            typeBinding.InitializationDelegate += initializationDelegate;
            return typeBinding;
        }
    }
}
