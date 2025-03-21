﻿using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingInjectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TApparent, TConcrete> Inject<TApparent, TConcrete>(
            this TypeBinding<TApparent, TConcrete> typeBinding, 
            InstanceContainerDelegate<TConcrete> injectionDelegate)
        {
            typeBinding.InjectionDelegate += injectionDelegate;
            return typeBinding;
        }
    }
}
