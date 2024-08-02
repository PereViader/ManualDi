using System;

namespace ManualDi.Main
{
    public static class DiContainerResolveExtensions
    {
        public static T Resolve<T>(this IDiContainer diContainer)
        {
            return (T)diContainer.Resolve(typeof(T), resolutionConstraints: null);
        }

        public static T Resolve<T>(this IDiContainer diContainer, Action<ResolutionConstraints> resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            resolution.Invoke(resolutionConstraints);

            return diContainer.Resolve<T>(resolutionConstraints);
        }

        public static T Resolve<T>(this IDiContainer diContainer, ResolutionConstraints resolutionConstraints)
        {
            return (T)diContainer.Resolve(typeof(T), resolutionConstraints);
        }
        
        public static object Resolve(this IDiContainer diContainer, Type type)
        {
            return diContainer.Resolve(type, resolutionConstraints: null);
        }

        public static object Resolve(this IDiContainer diContainer, Type type, Action<ResolutionConstraints> resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            resolution.Invoke(resolutionConstraints);

            return diContainer.Resolve(type, resolutionConstraints);
        }

        public static object Resolve(this IDiContainer diContainer, Type type, ResolutionConstraints? resolutionConstraints)
        {
            if (!diContainer.TryResolveContainer(type, resolutionConstraints, out object resolution))
            {
                throw new InvalidOperationException($"Could not resolve {type.FullName}");
            }

            return resolution;
        }
    }
}
