using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class DiContainerResolveAllExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<TConcrete> ResolveAll<TConcrete>(this IDiContainer diContainer)
        {
            return diContainer.ResolveAll<TConcrete>(resolutionConstraints: null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<TConcrete> ResolveAll<TInterface, TConcrete>(this IDiContainer diContainer)
        {
            return diContainer.ResolveAll<TInterface, TConcrete>(resolutionConstraints: null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<TInterface> ResolveAll<TInterface>(this IDiContainer diContainer, Action<ResolutionConstraints> resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            resolution.Invoke(resolutionConstraints);
            return diContainer.ResolveAll<TInterface>(resolutionConstraints);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<TConcrete> ResolveAll<TInterface, TConcrete>(this IDiContainer diContainer, Action<ResolutionConstraints> resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            resolution.Invoke(resolutionConstraints);
            return diContainer.ResolveAll<TInterface, TConcrete>(resolutionConstraints);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<TConcrete> ResolveAll<TConcrete>(this IDiContainer diContainer, ResolutionConstraints? resolutionConstraints)
        {
            return diContainer.ResolveAll<TConcrete>(typeof(TConcrete), resolutionConstraints);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<TConcrete> ResolveAll<TInterface, TConcrete>(this IDiContainer diContainer, ResolutionConstraints? resolutionConstraints)
        {
            return diContainer.ResolveAll<TConcrete>(typeof(TInterface), resolutionConstraints);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<object> ResolveAll(this IDiContainer diContainer, Type type)
        {
            return diContainer.ResolveAll<object>(type, resolutionConstraints: null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> ResolveAll<T>(this IDiContainer diContainer, Type type)
        {
            return diContainer.ResolveAll<T>(type, resolutionConstraints: null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<object> ResolveAll(this IDiContainer diContainer, Type type, Action<ResolutionConstraints> configureResolutionConstraints)
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureResolutionConstraints.Invoke(resolutionConstraints);
            return diContainer.ResolveAll<object>(type, resolutionConstraints);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> ResolveAll<T>(this IDiContainer diContainer, Type type, Action<ResolutionConstraints> configureResolutionConstraints)
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureResolutionConstraints.Invoke(resolutionConstraints);
            return diContainer.ResolveAll<T>(type, resolutionConstraints);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<object> ResolveAll(this IDiContainer diContainer, Type type, ResolutionConstraints? resolutionConstraints)
        {
            return diContainer.ResolveAll<object>(type, resolutionConstraints);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> ResolveAll<T>(this IDiContainer diContainer, Type type, ResolutionConstraints? resolutionConstraints)
        {
            var resolutions = new List<T>();
            diContainer.ResolveAllContainer(type, resolutionConstraints, resolutions);
            return resolutions;
        }
    }
}

