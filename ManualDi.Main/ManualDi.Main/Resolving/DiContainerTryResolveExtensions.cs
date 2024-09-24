using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class DiContainerTryResolveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve<T>(this IDiContainer diContainer, [MaybeNullWhen(false)] out T resolution)
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
        public static bool TryResolve<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate, [MaybeNullWhen(false)] out T resolution)
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
        public static bool TryResolve(this IDiContainer diContainer, Type type, [MaybeNullWhen(false)] out object resolution)
        {
            resolution = diContainer.ResolveContainer(type);
            return resolution is not null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate, [MaybeNullWhen(false)] out object resolution)
        {
            resolution = diContainer.ResolveContainer(type, filterBindingDelegate);
            return resolution is not null;
        }
    }
}
