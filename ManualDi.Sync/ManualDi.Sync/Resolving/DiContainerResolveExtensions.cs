using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class DiContainerResolveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IDiContainer diContainer)
        {
            var resolution = diContainer.ResolveContainer(typeof(T));
            if (resolution is null)
            {
                ThrowHelper.ThrowCouldNotResolveElement(typeof(T));
            }
            return (T)resolution;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
        {
            var resolution = diContainer.ResolveContainer(typeof(T), filterBindingDelegate);
            if (resolution is null)
            {
                ThrowHelper.ThrowCouldNotResolveElement(typeof(T));
            }
            return (T)resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Resolve(this IDiContainer diContainer, Type type)
        {
            var resolution = diContainer.ResolveContainer(type);
            if (resolution is null)
            {
                ThrowHelper.ThrowCouldNotResolveElement(type);
            }
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Resolve(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            var resolution = diContainer.ResolveContainer(type, filterBindingDelegate: filterBindingDelegate);
            if (resolution is null)
            {
                ThrowHelper.ThrowCouldNotResolveElement(type);
            }
            return resolution;
        }
    }
}
