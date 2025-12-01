using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class BindingContextExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Id(this BindingContext bindingContext, object id)
        {
            return object.Equals(bindingContext.Binding.Id, id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InjectedIntoType<T>(this BindingContext bindingContext)
        {
            return bindingContext.InjectedIntoBinding?.ConcreteType == typeof(T);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InjectedIntoType(this BindingContext bindingContext, Type type)
        {
            return bindingContext.InjectedIntoBinding?.ConcreteType == type;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InjectedIntoId(this BindingContext bindingContext, object id)
        {
            return object.Equals(bindingContext.InjectedIntoBinding?.Id, id);
        }
    }
}
