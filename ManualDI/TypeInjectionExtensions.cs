using System;
using System.Collections.Generic;
using ManualDI.TypeInjections;

namespace ManualDI
{
    public static class TypeInjectionExtensions
    {
        public static ITypeBinding<T> Inject<T>(this ITypeBinding<T> typeBinding, Action<T, IDiContainer> action)
        {
            if(typeBinding.TypeInjections == null)
            {
                typeBinding.TypeInjections = new List<ITypeInjection>();       
            }

            typeBinding.TypeInjections.Add(new MethodTypeInjection<T>(action));
            return typeBinding;
        }
    }
}
