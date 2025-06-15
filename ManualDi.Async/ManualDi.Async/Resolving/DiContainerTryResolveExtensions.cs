using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    public static class DiContainerTryResolveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve<T>(this IDiContainer diContainer, [NotNullWhen(true)] out T? resolution)
        {
            var result = diContainer.ResolveContainer(typeof(T));
            if (result is null)
            {
                resolution = default;
                return false;
            }

            resolution = (T)result;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate, [NotNullWhen(true)] out T? resolution)
        {
            var result = diContainer.ResolveContainer(typeof(T), filterBindingDelegate);
            if (result is null)
            {
                resolution = default;
                return false;
            }

            resolution = (T)result;
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve(this IDiContainer diContainer, Type type, [NotNullWhen(true)] out object? resolution)
        {
            resolution = diContainer.ResolveContainer(type);
            return resolution is not null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate, [NotNullWhen(true)] out object? resolution)
        {
            resolution = diContainer.ResolveContainer(type, filterBindingDelegate);
            return resolution is not null;
        }
    }
}
