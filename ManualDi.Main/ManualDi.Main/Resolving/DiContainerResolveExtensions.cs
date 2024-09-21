using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class DiContainerWouldResolveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve<T>(this IDiContainer diContainer)
        {
            return diContainer.WouldResolveContainer(typeof(T), filterBindingDelegate: null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve<T>(this IDiContainer diContainer, Action<ResolutionConstraints> configureResolutionConstraints)
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureResolutionConstraints.Invoke(resolutionConstraints);

            return diContainer.WouldResolveContainer(typeof(T), resolutionConstraints.FilterBindingDelegate);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve(this IDiContainer diContainer, Type type)
        {
            return diContainer.WouldResolveContainer(type, filterBindingDelegate: null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WouldResolve(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            return diContainer.WouldResolveContainer(type, filterBindingDelegate: filterBindingDelegate);
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
        public static T Resolve<T>(this IDiContainer diContainer, Action<ResolutionConstraints> configureResolutionConstraints)
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureResolutionConstraints.Invoke(resolutionConstraints);

            var resolution = diContainer.ResolveContainer(typeof(T), resolutionConstraints.FilterBindingDelegate);
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
