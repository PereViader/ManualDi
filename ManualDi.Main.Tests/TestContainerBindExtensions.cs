using System;

namespace ManualDi.Main.Tests
{
    public static class TestContainerBindExtensions
    {
        public static void BindAndFinish<T>(this IDiContainer diContainer, Action<ITypeBinding<T, T>> typeSetup)
        {
            BindAndFinish<T, T>(diContainer, typeSetup);
        }

        public static T BindFinishAndResolve<T>(this IDiContainer diContainer, Action<ITypeBinding<T, T>> typeSetup)
        {
            return BindFinishAndResolve<T, T>(diContainer, typeSetup);
        }

        public static void BindAndFinish<TInterface, TConcrete>(
            this IDiContainer diContainer,
            Action<ITypeBinding<TInterface, TConcrete>> typeSetup
            )
        {
            diContainer.Bind(typeSetup);
            diContainer.FinishBinding();
        }

        public static TInterface BindFinishAndResolve<TInterface, TConcrete>(
            this IDiContainer diContainer,
            Action<ITypeBinding<TInterface, TConcrete>> typeSetup
            )
        {
            diContainer.BindAndFinish(typeSetup);
            return diContainer.Resolve<TInterface>();
        }
    }
}
