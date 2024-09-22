using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class BindingContextExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Id(this BindingContext bindingContext, object id)
        {
            return bindingContext.TypeBinding.Id == id;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InjectedIntoType<T>(this BindingContext bindingContext)
        {
            return bindingContext.InjectedIntoTypeBinding?.ConcreteType == typeof(T);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InjectedIntoType(this BindingContext bindingContext, Type type)
        {
            return bindingContext.InjectedIntoTypeBinding?.ConcreteType == type;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InjectedIntoId(this BindingContext bindingContext, object id)
        {
            return bindingContext.InjectedIntoTypeBinding?.Id == id;
        }
    }
}
