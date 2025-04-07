using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class DiContainerWouldResolveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve<T>(this IDiContainer diContainer)
        {
            return diContainer.WouldResolveContainer(typeof(T), null, null, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
        {
            return diContainer.WouldResolveContainer(typeof(T), filterBindingDelegate, null, null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve<TResolve, TInjectedInto>(this IDiContainer diContainer, FilterBindingDelegate? filterBindingDelegate = null,
            FilterBindingDelegate? injectedIntoFilterBindingDelegate = null)
        {
            return diContainer.WouldResolveContainer(typeof(TResolve), filterBindingDelegate, typeof(TInjectedInto), injectedIntoFilterBindingDelegate);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve(this IDiContainer diContainer, Type type)
        {
            return diContainer.WouldResolveContainer(type, null, null, null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            return diContainer.WouldResolveContainer(type, filterBindingDelegate, null, null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve(this IDiContainer diContainer, Type type, FilterBindingDelegate? filterBindingDelegate,
            Type overrideInjectedIntoType, FilterBindingDelegate? overrideFilterBindingDelegate)
        {
            return diContainer.WouldResolveContainer(type, filterBindingDelegate, overrideInjectedIntoType, overrideFilterBindingDelegate);
        }
    }
}