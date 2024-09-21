using System.Runtime.CompilerServices;

namespace ManualDi.Main
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeTypeBinding Initialize(
            this UnsafeTypeBinding typeBinding,
            InstanceContainerDelegate<object> initializationDelegate
            )
        {
            typeBinding.InitializationDelegate += initializationDelegate;
            return typeBinding;
        }
    }
}
