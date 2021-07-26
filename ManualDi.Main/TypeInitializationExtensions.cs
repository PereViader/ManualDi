using ManualDi.Main.Initialization;

namespace ManualDi.Main
{
    public static class TypeInitializationExtensions
    {
        public static ITypeBinding<T> Initialize<T>(this ITypeBinding<T> typeBinding, InitializationDelegate<T> initializationDelegate)
        {
            typeBinding.BindingInitialization = new BindingInitialization<T>(initializationDelegate);
            return typeBinding;
        }
    }
}
