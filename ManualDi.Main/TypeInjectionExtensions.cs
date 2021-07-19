using ManualDi.Main.TypeInjections;
using System.Collections.Generic;

namespace ManualDi.Main
{
    public delegate void InjectionDelegate<T>(T instance, IDiContainer diContainer);

    public static class TypeInjectionExtensions
    {
        public static ITypeBinding<T> Inject<T>(this ITypeBinding<T> typeBinding, InjectionDelegate<T> injectionDelegate)
        {
            if (typeBinding.TypeInjections == null)
            {
                typeBinding.TypeInjections = new List<ITypeInjection>();
            }

            typeBinding.TypeInjections.Add(new MethodTypeInjection<T>(injectionDelegate));
            return typeBinding;
        }
    }
}
