using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingInjectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> Inject<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync, 
            InstanceContainerDelegate<TConcrete> injectionDelegate)
        {
            typeBindingSync.InjectionDelegate += injectionDelegate;
            return typeBindingSync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> Inject<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync, 
            InstanceContainerDelegate<TConcrete> injectionDelegate)
        {
            typeBindingAsync.InjectionDelegate += injectionDelegate;
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> InjectAsync<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync, 
            InstanceContainerAsyncDelegate<TConcrete> injectionAsyncDelegate)
        {
            typeBindingAsync.InjectionAsyncDelegate += injectionAsyncDelegate;
            return typeBindingAsync;
        }
    }
}
