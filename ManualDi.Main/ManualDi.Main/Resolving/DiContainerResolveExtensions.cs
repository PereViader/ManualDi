using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
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
    
    public static class DiContainerResolveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IDiContainer diContainer)
        {
            var resolution = diContainer.ResolveContainer(typeof(T), filterBindingDelegate: null);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {typeof(T).FullName}");
            }
            return (T)resolution;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
        {
            var resolution = diContainer.ResolveContainer(typeof(T), filterBindingDelegate);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {typeof(T).FullName}");
            }
            return (T)resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Resolve(this IDiContainer diContainer, Type type)
        {
            var resolution = diContainer.ResolveContainer(type, filterBindingDelegate: null);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {type.FullName}");
            }
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Resolve(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            var resolution = diContainer.ResolveContainer(type, filterBindingDelegate: filterBindingDelegate);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {type.FullName}");
            }
            return resolution;
        }
    }
}
