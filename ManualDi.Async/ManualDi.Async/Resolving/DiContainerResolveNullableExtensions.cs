using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ManualDi.Async
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
        public static async Task<T?> ResolveNullableAsync<T>(this IDiContainer diContainer)
            where T : class
        {
            var resolution = diContainer.ResolveContainer(typeof(Task<T>));
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve async element of type {typeof(T).FullName}");
            }
            var result = await (Task<object?>)resolution;
            if (result is null)
            {
                return default;
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
        public static async Task<T?> ResolveNullableValueAsync<T>(this IDiContainer diContainer)
            where T : struct
        {
            var resolution = diContainer.ResolveContainer(typeof(Task<T>));
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve async element of type {typeof(T).FullName}");
            }
            var result = await (Task<object?>)resolution;
            if (result is null)
            {
                return default;
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
        public static async Task<T?> ResolveNullableAsync<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
            where T : class
        {
            var resolution = diContainer.ResolveContainer(typeof(Task<T>), filterBindingDelegate);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve async element of type {typeof(T).FullName}");
            }
            var result = await (Task<object?>)resolution;
            if (result is null)
            {
                return default;
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
        public static async Task<T?> ResolveNullableValueAsync<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
            where T : struct
        {
            var resolution = diContainer.ResolveContainer(typeof(Task<T>), filterBindingDelegate);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve async element of type {typeof(T).FullName}");
            }
            var result = await (Task<object?>)resolution;
            if (result is null)
            {
                return default;
            }
            return (T)result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? ResolveNullable(this IDiContainer diContainer, Type type)
        {
            return diContainer.ResolveContainer(type);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<object?> ResolveNullableAsync(this IDiContainer diContainer, Type type)
        {
            var resolution = diContainer.ResolveContainer(typeof(Task<>).MakeGenericType(type));
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve async element of type {type.FullName}");
            }
            return (Task<object?>)resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? ResolveNullable(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            return diContainer.ResolveContainer(type, filterBindingDelegate);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<object?> ResolveNullableAsync(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            var resolution = diContainer.ResolveContainer(typeof(Task<>).MakeGenericType(type), filterBindingDelegate);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve async element of type {type.FullName}");
            }
            return (Task<object?>)resolution;
        }
    }
}