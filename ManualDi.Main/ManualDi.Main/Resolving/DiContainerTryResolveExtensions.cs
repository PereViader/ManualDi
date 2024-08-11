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
            if (!diContainer.TryResolveContainer(typeof(T), resolutionConstraints: null, out var result))
            {
                resolution = default;
                return false;
            }

            resolution = (T)result;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve<T>(this IDiContainer diContainer, Action<ResolutionConstraints> configureReslutionConstraints, [MaybeNullWhen(false)] out T resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureReslutionConstraints.Invoke(resolutionConstraints);

            return diContainer.TryResolve<T>(resolutionConstraints, out resolution);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve<T>(this IDiContainer diContainer, ResolutionConstraints resolutionConstraints, [MaybeNullWhen(false)] out T resolution)
        {
            if (!diContainer.TryResolve(typeof(T), resolutionConstraints, out var result))
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
            return diContainer.TryResolveContainer(type, resolutionConstraints: null, out resolution);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve(this IDiContainer diContainer, Type type, Action<ResolutionConstraints> configureResolutionConstraints, [MaybeNullWhen(false)] out object resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureResolutionConstraints.Invoke(resolutionConstraints);

            return diContainer.TryResolveContainer(type, resolutionConstraints, out resolution);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve(this IDiContainer diContainer, Type type, ResolutionConstraints resolutionConstraints, [MaybeNullWhen(false)] out object resolution)
        {
            return diContainer.TryResolveContainer(type, resolutionConstraints, out resolution);
        }
    }
}
