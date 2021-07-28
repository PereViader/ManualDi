using System;

namespace ManualDi.Main
{
    public delegate Action GetDisposeDelegate<T>(T instance);

    public static class TypeDisposableExtensions
    {
        public static ITypeBinding<T> RegisterDispose<T>(this ITypeBinding<T> typeBinding)
            where T : IDisposable
        {
            return RegisterDispose(typeBinding, GetDispose);
        }

        private static Action GetDispose<T>(T instance) where T : IDisposable
        {
            return instance.Dispose;
        }

        public static ITypeBinding<T> RegisterDispose<T>(this ITypeBinding<T> typeBinding, GetDisposeDelegate<T> getDisposeDelegate)
        {
            typeBinding.Inject((o, c) => c.QueueDispose(getDisposeDelegate.Invoke(o)));
            return typeBinding;
        }
    }
}
