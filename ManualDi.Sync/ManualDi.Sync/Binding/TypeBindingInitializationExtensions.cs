using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class TypeBindingInitializationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TApparent, TConcrete> Initialize<TApparent, TConcrete>(
            this TypeBinding<TApparent, TConcrete> typeBinding,
            InstanceContainerDelegate<TConcrete> initializationDelegate
            )
        {
            typeBinding.InitializationDelegate += initializationDelegate;
            return typeBinding;
        }
    }
}
