using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingFromExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> FromContainer<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding
            )
        {
            typeBinding.CreateConcreteDelegate = c => c.Resolve<TConcrete>();
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> FromContainer<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            Action<ResolutionConstraints> constraints
            )
        {
            typeBinding.CreateConcreteDelegate = c => c.Resolve<TConcrete>(constraints);
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> FromInstance<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            TConcrete instance
            )
        {
            typeBinding.CreateConcreteDelegate = _ => instance;
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> FromMethod<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            CreateDelegate<TConcrete> createDelegate
            )
        {
            typeBinding.CreateConcreteDelegate = createDelegate;
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> Lazy<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding
            )
        {
            typeBinding.IsLazy = true;
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> NonLazy<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding
            )
        {
            typeBinding.IsLazy = false;
            return typeBinding;
        }
    }
}
