using System;
using ManualDI.TypeFactories;

namespace ManualDI
{
    public static class TypeFactoryExtensions
    {
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
