using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class DiContainerResolveNullableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullable<T>(this IDiContainer diContainer)
            where T : class
        {
            var result = diContainer.ResolveContainer(typeof(T));
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
            var result = diContainer.ResolveContainer(typeof(T));
            if (result is null)
            {
                return null;
            }

            return (T)result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullable<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
            where T : class
        {
            var result = diContainer.ResolveContainer(typeof(T), filterBindingDelegate);
            if (result is null)
            {
                return null;
            }

            return (T)result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ResolveNullableValue<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
            where T : struct
        {
            var result = diContainer.ResolveContainer(typeof(T), filterBindingDelegate);
            if (result is null)
            {
                return null;
            }

            return (T)result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? ResolveNullable(this IDiContainer diContainer, Type type)
        {
            return diContainer.ResolveContainer(type);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? ResolveNullable(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            return diContainer.ResolveContainer(type, filterBindingDelegate);
        }
    }
}