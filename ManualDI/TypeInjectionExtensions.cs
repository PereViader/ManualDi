using System;
using ManualDI.TypeInjections;

namespace ManualDI
{
    public static class TypeInjectionExtensions
    {
        public static ITypeBinding<T> Inject<T>(this ITypeBinding<T> typeBinding, Action<T, IDiContainer> action)
        {
            typeBinding.TypeInjection = new MethodTypeInjection<T>(action);
            return typeBinding;
        }
    }
}
