using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingInitializationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> Initialize<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            InitializeDelegate<TConcrete> initializationDelegate
            )
        {
            typeBindingSync.InitializationDelegate += initializationDelegate;
            return typeBindingSync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> Initialize<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingSync,
            InitializeDelegate<TConcrete> initializationDelegate
        )
        {
            typeBindingSync.InitializationDelegate += initializationDelegate;
            return typeBindingSync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> InitializeAsync<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingSync,
            InitializeAsyncDelegate<TConcrete> initializationAsyncDelegate
        )
        {
            typeBindingSync.InitializationAsyncDelegate += initializationAsyncDelegate;
            return typeBindingSync;
        }
    }
}
