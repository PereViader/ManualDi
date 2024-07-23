using System;
using System.Diagnostics.CodeAnalysis;

namespace ManualDi.Main
{
    public static class DiContainerTryResolveExtensions
    {
        public static bool TryResolve<T>(this IDiContainer diContainer, [MaybeNullWhen(false)] out T resolution)
        {
            if (!diContainer.TryResolveContainer(typeof(T), resolutionConstraints: null, out object result))
            {
                resolution = default;
                return false;
            }

            resolution = (T)result;
            return true;
        }

        public static bool TryResolve<T>(this IDiContainer diContainer, Action<IResolutionConstraints> configureReslutionConstraints, [MaybeNullWhen(false)] out T resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureReslutionConstraints.Invoke(resolutionConstraints);

            return diContainer.TryResolve<T>(resolutionConstraints, out resolution);
        }

        public static bool TryResolve<T>(this IDiContainer diContainer, IResolutionConstraints resolutionConstraints, [MaybeNullWhen(false)] out T resolution)
        {
            if (!diContainer.TryResolve(typeof(T), resolutionConstraints, out var result))
            {
                resolution = default;
                return false;
            }

            resolution = (T)result;
            return true;
        }
        
        public static bool TryResolve(this IDiContainer diContainer, Type type, [MaybeNullWhen(false)] out object resolution)
        {
            return diContainer.TryResolveContainer(type, resolutionConstraints: null, out resolution);
        }

        public static bool TryResolve(this IDiContainer diContainer, Type type, Action<IResolutionConstraints> configureResolutionConstraints, [MaybeNullWhen(false)] out object resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            configureResolutionConstraints.Invoke(resolutionConstraints);

            return diContainer.TryResolveContainer(type, resolutionConstraints, out resolution);
        }

        public static bool TryResolve(this IDiContainer diContainer, Type type, IResolutionConstraints resolutionConstraints, [MaybeNullWhen(false)] out object resolution)
        {
            return diContainer.TryResolveContainer(type, resolutionConstraints, out resolution);
        }
    }
    
    public static class DiContainerResolveNullableExtensions
    {
        public static T? ResolveNullable<T>(this IDiContainer diContainer)
            where T : class
        {
            diContainer.TryResolve<T>(out var res);
            return res;
        }
        
        public static T? ResolveNullableValue<T>(this IDiContainer diContainer)
            where T : struct
        {
            return diContainer.TryResolve<T>(out var res) ? res : null;
        }

        public static T? ResolveNullable<T>(this IDiContainer diContainer, Action<IResolutionConstraints> configureReslutionConstraints)
            where T : class
        {
            diContainer.TryResolve<T>(configureReslutionConstraints, out var res);
            return res;
        }
        
        public static T? ResolveNullableValue<T>(this IDiContainer diContainer, Action<IResolutionConstraints> configureReslutionConstraints)
            where T : struct
        {
            return diContainer.TryResolve<T>(configureReslutionConstraints, out var res) ? res : null;
        }

        public static T? ResolveNullable<T>(this IDiContainer diContainer, IResolutionConstraints resolutionConstraints)
            where T : class
        {
            diContainer.TryResolve<T>(resolutionConstraints, out var res);
            return res;
        }
        
        public static T? ResolveNullableValue<T>(this IDiContainer diContainer, IResolutionConstraints resolutionConstraints)
            where T : struct
        {
            return diContainer.TryResolve<T>(resolutionConstraints, out var res) ? res : null;
        }
        
        public static object? ResolveNullable(this IDiContainer diContainer, Type type)
        {
            diContainer.TryResolve(type, out var res);
            return res;
        }

        public static object? ResolveNullable(this IDiContainer diContainer, Type type, Action<IResolutionConstraints> configureResolutionConstraints)
        {
            diContainer.TryResolve(type, configureResolutionConstraints, out var res);
            return res;
        }

        public static object? ResolveNullable(this IDiContainer diContainer, Type type, IResolutionConstraints resolutionConstraints)
        {
            diContainer.TryResolve(type, resolutionConstraints, out var res);
            return res;
        }
    }
}
