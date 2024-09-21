using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class DiContainerResolveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IDiContainer diContainer)
        {
            var resolution = diContainer.ResolveContainer(typeof(T), isValidBindingDelegate: null);
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

            var resolution = diContainer.ResolveContainer(typeof(T), resolutionConstraints.IsValidBindingDelegate);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {typeof(T).FullName}");
            }
            return (T)resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Resolve(this IDiContainer diContainer, Type type)
        {
            var resolution = diContainer.ResolveContainer(type, isValidBindingDelegate: null);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {type.FullName}");
            }
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Resolve(this IDiContainer diContainer, Type type, IsValidBindingDelegate isValidBindingDelegate)
        {
            var resolution = diContainer.ResolveContainer(type, isValidBindingDelegate: isValidBindingDelegate);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {type.FullName}");
            }
            return resolution;
        }
    }
}
