using System;

namespace ManualDi.Main
{
    public delegate Action GetDisposeDelegate<T>(T instance, IDiContainer container);

    public static class TypeDisposableExtensions
    {
        public static ITypeBinding<T> RegisterDispose<T>(this ITypeBinding<T> typeBinding)
            where T : IDisposable
        {
            return RegisterDispose(typeBinding, GetDispose);
        }

        private static Action GetDispose<T>(T instance, IDiContainer container) where T : IDisposable
        {
            return instance.Dispose;
        }

        public static ITypeBinding<T> RegisterDispose<T>(this ITypeBinding<T> typeBinding, GetDisposeDelegate<T> getDisposeDelegate)
        {
            typeBinding.Inject((o, c) => c.QueueDispose(getDisposeDelegate.Invoke(o, c)));
            return typeBinding;
        }
    }
}
