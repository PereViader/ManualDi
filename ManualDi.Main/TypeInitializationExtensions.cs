using ManualDi.Main.Initialization;
using System;

namespace ManualDi.Main
{
    public static class TypeInitializationExtensions
    {
        public static ITypeBinding<T> Initialize<T>(this ITypeBinding<T> typeBinding, InitializationDelegate<T> initializationDelegate)
        {
            if (typeBinding.BindingInitialization != null)
            {
                throw new InvalidOperationException($"Initialize can only be called once on {nameof(ITypeBinding)} of type {typeof(T).FullName}");
            }

            typeBinding.BindingInitialization = new BindingInitialization<T>(initializationDelegate);
            return typeBinding;
        }
    }
}
