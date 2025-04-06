using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class DiContainerResolveAllExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> ResolveAll<T>(this IDiContainer diContainer)
        {
            var resolutions = new List<T>();
            diContainer.ResolveAllContainer(typeof(T), filterBindingDelegate: null, resolutions);
            return resolutions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> ResolveAll<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
        {
            var resolutions = new List<T>();
            diContainer.ResolveAllContainer(typeof(T), filterBindingDelegate, resolutions);
            return resolutions;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<object> ResolveAll(this IDiContainer diContainer, Type type)
        {
            var resolutions = new List<object>();
            diContainer.ResolveAllContainer(type, filterBindingDelegate: null, resolutions);
            return resolutions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<object> ResolveAll(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            var resolutions = new List<object>();
            diContainer.ResolveAllContainer(type, filterBindingDelegate, resolutions);
            return resolutions;
        }
    }
}

