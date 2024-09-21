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
            var result = diContainer.ResolveContainer(typeof(T), filterBindingDelegate: null);
            if (result is null)
            {
                resolution = default;
                return false;
            }

            resolution = (T)result;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve<T>(this IDiContainer diContainer, Action<ResolutionConstraints> configureResolutionConstraints, [MaybeNullWhen(false)] out T resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureResolutionConstraints.Invoke(resolutionConstraints);

            var result = diContainer.ResolveContainer(typeof(T), resolutionConstraints.FilterBindingDelegate);
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
            resolution = diContainer.ResolveContainer(type, filterBindingDelegate: null);
            return resolution is not null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve(this IDiContainer diContainer, Type type, Action<ResolutionConstraints> configureResolutionConstraints, [MaybeNullWhen(false)] out object resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureResolutionConstraints.Invoke(resolutionConstraints);

            resolution = diContainer.ResolveContainer(type, resolutionConstraints.FilterBindingDelegate);
            return resolution is not null;
        }
    }
}
