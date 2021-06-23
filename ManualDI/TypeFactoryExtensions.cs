using ManualDI.TypeFactories;
using System;
using System.Collections.Generic;

namespace ManualDI
{
    public static class TypeFactoryExtensions
    {
        public static ITypeBinding<List<Y>> FromContainerAll<T, Y>(this ITypeBinding<List<Y>> typeBinding)
            where T : Y
        {
            typeBinding.Factory = new ContainerAllTypeFactory<T, Y>();
            return typeBinding;
        }

        public static ITypeBinding<List<T>> FromContainerAll<T>(this ITypeBinding<List<T>> typeBinding)
        {
            return FromContainerAll<T, T>(typeBinding);
        }

        public static ITypeBinding<List<Y>> FromContainerAll<T, Y>(this ITypeBinding<List<Y>> typeBinding, Action<IResolutionConstraints> constraints)
            where T : Y
        {
            typeBinding.Factory = new ConstraintAllContainerTypeFactory<T, Y>(constraints);
            return typeBinding;
        }

        public static ITypeBinding<List<T>> FromContainerAll<T>(this ITypeBinding<List<T>> typeBinding, Action<IResolutionConstraints> constraints)
        {
            return FromContainerAll<T, T>(typeBinding, constraints);
        }

        public static ITypeBinding<T> FromContainer<T>(this ITypeBinding<T> typeBinding)
        {
            return FromContainer<T, T>(typeBinding);
        }

        public static ITypeBinding<Y> FromContainer<T, Y>(this ITypeBinding<Y> typeBinding)
            where T : Y
        {
            typeBinding.Factory = new ContainerTypeFactory<T, Y>();
            return typeBinding;
        }

        public static ITypeBinding<T> FromContainer<T>(this ITypeBinding<T> typeBinding, Action<IResolutionConstraints> constraints)
        {
            return FromContainer<T, T>(typeBinding, constraints);
        }

        public static ITypeBinding<Y> FromContainer<T, Y>(this ITypeBinding<Y> typeBinding, Action<IResolutionConstraints> constraints)
            where T : Y
        {
            typeBinding.Factory = new ConstraintContainerTypeFactory<T, Y>(constraints);
            return typeBinding;
        }

        public static ITypeBinding<T> FromInstance<T>(this ITypeBinding<T> typeBinding, T instance)
        {
            typeBinding.Factory = new InstanceTypeFactory<T>(instance);
            return typeBinding;
        }

        public static ITypeBinding<T> FromMethod<T>(this ITypeBinding<T> typeBinding, Func<IDiContainer, T> func)
        {
            typeBinding.Factory = new MethodTypeFactory<T>(func);
            return typeBinding;
        }

        public static ITypeBinding<T> FromFactory<TFactory, T>(this ITypeBinding<T> typeBinding)
            where TFactory : IFactory<T>
        {
            typeBinding.Factory = new FactoryTypeFactory<TFactory, T>();
            return typeBinding;
        }

        public static ITypeBinding<T> Lazy<T>(this ITypeBinding<T> typeBinding)
        {
            typeBinding.IsLazy = true;
            return typeBinding;
        }

        public static ITypeBinding<T> NonLazy<T>(this ITypeBinding<T> typeBinding)
        {
            typeBinding.IsLazy = false;
            return typeBinding;
        }
    }
}
