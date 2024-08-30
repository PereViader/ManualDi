using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class DiContainerResolveNullableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullable<T>(this IDiContainer diContainer)
            where T : class
        {
            var result = diContainer.ResolveContainer(typeof(T), resolutionConstraints: null);
            if (result is null)
            {
                return null;
            }

            return (T)result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullableValue<T>(this IDiContainer diContainer)
            where T : struct
        {
            var result = diContainer.ResolveContainer(typeof(T), resolutionConstraints: null);
            if (result is null)
            {
                return null;
            }

            return (T)result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullable<T>(this IDiContainer diContainer, Action<ResolutionConstraints> configureReslutionConstraints)
            where T : class
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureReslutionConstraints.Invoke(resolutionConstraints);

            var result = diContainer.ResolveContainer(typeof(T), resolutionConstraints);
            if (result is null)
            {
                return null;
            }

            return (T)result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullableValue<T>(this IDiContainer diContainer, Action<ResolutionConstraints> configureReslutionConstraints)
            where T : struct
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureReslutionConstraints.Invoke(resolutionConstraints);

            var result = diContainer.ResolveContainer(typeof(T), resolutionConstraints);
            if (result is null)
            {
                return null;
            }

            return (T)result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? ResolveNullable(this IDiContainer diContainer, Type type)
        {
            return diContainer.ResolveContainer(type, resolutionConstraints: null);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? ResolveNullable<T>(this IDiContainer diContainer, Type type, Action<ResolutionConstraints> configureReslutionConstraints)
            where T : class
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureReslutionConstraints.Invoke(resolutionConstraints);

            return diContainer.ResolveContainer(type, resolutionConstraints);
        }
    }
}