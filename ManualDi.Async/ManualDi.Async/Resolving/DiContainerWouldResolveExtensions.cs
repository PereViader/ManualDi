using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ManualDi.Async
{
    public static class DiContainerWouldResolveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve<T>(this IDiContainer diContainer)
        {
            return diContainer.WouldResolveContainer(typeof(T), null, null, null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolveAsync<T>(this IDiContainer diContainer)
        {
            return diContainer.WouldResolveContainer(typeof(Task<T>), null, null, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
        {
            return diContainer.WouldResolveContainer(typeof(T), filterBindingDelegate, null, null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolveAsync<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
        {
            return diContainer.WouldResolveContainer(typeof(Task<T>), filterBindingDelegate, null, null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve<TResolve, TInjectedInto>(this IDiContainer diContainer, FilterBindingDelegate? filterBindingDelegate = null,
            FilterBindingDelegate? injectedIntoFilterBindingDelegate = null)
        {
            return diContainer.WouldResolveContainer(typeof(TResolve), filterBindingDelegate, typeof(TInjectedInto), injectedIntoFilterBindingDelegate);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolveAsync<TResolve, TInjectedInto>(this IDiContainer diContainer, FilterBindingDelegate? filterBindingDelegate = null,
            FilterBindingDelegate? injectedIntoFilterBindingDelegate = null)
        {
            return diContainer.WouldResolveContainer(typeof(Task<TResolve>), filterBindingDelegate, typeof(TInjectedInto), injectedIntoFilterBindingDelegate);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve(this IDiContainer diContainer, Type type)
        {
            return diContainer.WouldResolveContainer(type, null, null, null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolveAsync(this IDiContainer diContainer, Type type)
        {
            return diContainer.WouldResolveContainer(typeof(Task<>).MakeGenericType(type), null, null, null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            return diContainer.WouldResolveContainer(type, filterBindingDelegate, null, null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolveAsync(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            return diContainer.WouldResolveContainer(typeof(Task<>).MakeGenericType(type), filterBindingDelegate, null, null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve(this IDiContainer diContainer, Type type, FilterBindingDelegate? filterBindingDelegate,
            Type overrideInjectedIntoType, FilterBindingDelegate? overrideFilterBindingDelegate)
        {
            return diContainer.WouldResolveContainer(type, filterBindingDelegate, overrideInjectedIntoType, overrideFilterBindingDelegate);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolveAsync(this IDiContainer diContainer, Type type, FilterBindingDelegate? filterBindingDelegate,
            Type overrideInjectedIntoType, FilterBindingDelegate? overrideFilterBindingDelegate)
        {
            return diContainer.WouldResolveContainer(typeof(Task<>).MakeGenericType(type), filterBindingDelegate, overrideInjectedIntoType, overrideFilterBindingDelegate);
        }
    }
}