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
            diContainer.TryResolve<T>(out var res);
            return res;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullableValue<T>(this IDiContainer diContainer)
            where T : struct
        {
            return diContainer.TryResolve<T>(out var res) ? res : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullable<T>(this IDiContainer diContainer, Action<ResolutionConstraints> configureReslutionConstraints)
            where T : class
        {
            diContainer.TryResolve<T>(configureReslutionConstraints, out var res);
            return res;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullableValue<T>(this IDiContainer diContainer, Action<ResolutionConstraints> configureReslutionConstraints)
            where T : struct
        {
            return diContainer.TryResolve<T>(configureReslutionConstraints, out var res) ? res : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullable<T>(this IDiContainer diContainer, ResolutionConstraints resolutionConstraints)
            where T : class
        {
            diContainer.TryResolve<T>(resolutionConstraints, out var res);
            return res;
        }
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullableValue<T>(this IDiContainer diContainer, ResolutionConstraints resolutionConstraints)
            where T : struct
        {
            return diContainer.TryResolve<T>(resolutionConstraints, out var res) ? res : null;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? ResolveNullable(this IDiContainer diContainer, Type type)
        {
            diContainer.TryResolve(type, out var res);
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? ResolveNullable(this IDiContainer diContainer, Type type, Action<ResolutionConstraints> configureResolutionConstraints)
        {
            diContainer.TryResolve(type, configureResolutionConstraints, out var res);
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? ResolveNullable(this IDiContainer diContainer, Type type, ResolutionConstraints resolutionConstraints)
        {
            diContainer.TryResolve(type, resolutionConstraints, out var res);
            return res;
        }
    }
}