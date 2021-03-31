using ManualDI.TypeFactories;
using System;
using System.Collections.Generic;

namespace ManualDI
{
    public static class TypeFactoryExtensions
    {
        public static ITypeBinding<List<T>> FromContainerAll<T>(this ITypeBinding<List<T>> typeBinding)
        {
            typeBinding.Factory = new ContainerAllTypeFactory<T>();
            return typeBinding;
        }

        public static ITypeBinding<List<T>> FromContainerAll<T>(this ITypeBinding<List<T>> typeBinding, Action<IResolutionConstraints> constraints)
        {
            typeBinding.Factory = new ConstraintAllContainerTypeFactory<T>(constraints);
            return typeBinding;
        }

        public static ITypeBinding<T> FromContainer<T>(this ITypeBinding<T> typeBinding)
        {
            typeBinding.Factory = new ContainerTypeFactory<T>();
            return typeBinding;
        }

        public static ITypeBinding<T> FromContainer<T>(this ITypeBinding<T> typeBinding, Action<IResolutionConstraints> constraints)
        {
            typeBinding.Factory = new ConstraintContainerTypeFactory<T>(constraints);
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
    }
}
