using System;

namespace ManualDi.Tests
{
    public static class TestContainerBindExtensions
    {
        public static void BindAndFinish<T>(this IDiContainer diContainer, Action<ITypeBinding<T>> typeSetup)
        {
            diContainer.Bind<T>(typeSetup);
            diContainer.FinishBinding();
        }

        public static T BindFinishAndResolve<T>(this IDiContainer diContainer, Action<ITypeBinding<T>> typeSetup)
        {
            diContainer.BindAndFinish(typeSetup);
            return diContainer.Resolve<T>();
        }
    }
}
