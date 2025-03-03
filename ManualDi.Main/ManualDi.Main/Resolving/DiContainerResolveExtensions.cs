﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    public static class DiContainerResolveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IDiContainer diContainer)
        {
            var resolution = diContainer.ResolveContainer(typeof(T));
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {typeof(T).FullName}");
            }
            return (T)resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<T> ResolveAsync<T>(this IDiContainer diContainer)
        {
            var resolution = diContainer.ResolveContainer(typeof(Task<T>));
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {typeof(T).FullName}");
            }
            var result = await (Task<object?>)resolution;
            if (result is null)
            {
                throw new InvalidOperationException($"Could not resolve async element of type {typeof(T).FullName}");
            }
            return (T)result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
        {
            var resolution = diContainer.ResolveContainer(typeof(T), filterBindingDelegate);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {typeof(T).FullName}");
            }
            return (T)resolution;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<T> ResolveAsync<T>(this IDiContainer diContainer, FilterBindingDelegate filterBindingDelegate)
        {
            var resolution = diContainer.ResolveContainer(typeof(Task<T>), filterBindingDelegate);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {typeof(T).FullName}");
            }
            var result = await (Task<object?>)resolution;
            if (result is null)
            {
                throw new InvalidOperationException($"Could not resolve async element of type {typeof(T).FullName}");
            }
            return (T)result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Resolve(this IDiContainer diContainer, Type type)
        {
            var resolution = diContainer.ResolveContainer(type);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {type.FullName}");
            }
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<object> ResolveAsync(this IDiContainer diContainer, Type type)
        {
            var resolution = diContainer.ResolveContainer(typeof(Task<>).MakeGenericType(type));
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {type.FullName}");
            }
            var result = await (Task<object?>)resolution;
            if (result is null)
            {
                throw new InvalidOperationException($"Could not resolve async element of type {type.FullName}");
            }
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Resolve(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            var resolution = diContainer.ResolveContainer(type, filterBindingDelegate);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {type.FullName}");
            }
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<object> ResolveAsync(this IDiContainer diContainer, Type type, FilterBindingDelegate filterBindingDelegate)
        {
            var resolution = diContainer.ResolveContainer(typeof(Task<>).MakeGenericType(type), filterBindingDelegate);
            if (resolution is null)
            {
                throw new InvalidOperationException($"Could not resolve element of type {type.FullName}");
            }
            var result = await (Task<object?>)resolution;
            if (result is null)
            {
                throw new InvalidOperationException($"Could not resolve async element of type {type.FullName}");
            }
            return result;
        }
    }
}
