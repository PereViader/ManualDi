using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class DiContainerResolveAllExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> ResolveAll<T>(this IDiContainer diContainer)
        {
            var resolutions = new List<T>();
            diContainer.ResolveAllContainer(typeof(T), isValidBindingDelegate: null, resolutions);
            return resolutions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> ResolveAll<T>(this IDiContainer diContainer, Action<ResolutionConstraints> configureResolutionConstraints)
        {
            var resolutions = new List<T>();
            var resolutionConstraints = new ResolutionConstraints();
            configureResolutionConstraints.Invoke(resolutionConstraints);
            diContainer.ResolveAllContainer(typeof(T), resolutionConstraints.IsValidBindingDelegate, resolutions);
            return resolutions;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<object> ResolveAll(this IDiContainer diContainer, Type type)
        {
            var resolutions = new List<object>();
            diContainer.ResolveAllContainer(type, isValidBindingDelegate: null, resolutions);
            return resolutions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<object> ResolveAll(this IDiContainer diContainer, Type type, Action<ResolutionConstraints> configureResolutionConstraints)
        {
            var resolutions = new List<object>();
            var resolutionConstraints = new ResolutionConstraints();
            configureResolutionConstraints.Invoke(resolutionConstraints);
            diContainer.ResolveAllContainer(type, resolutionConstraints.IsValidBindingDelegate, resolutions);
            return resolutions;
        }
    }
}

