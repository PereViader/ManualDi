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
            var result = diContainer.ResolveContainer(typeof(T), resolutionConstraints: null);
            if (result is null)
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

            var result = diContainer.ResolveContainer(typeof(T), resolutionConstraints);
            if (result is null)
            {
                resolution = default;
                return false;
            }

            resolution = (T)result;
            return true;
        }
    }
}
