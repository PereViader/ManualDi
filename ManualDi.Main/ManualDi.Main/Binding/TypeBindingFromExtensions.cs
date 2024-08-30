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
        public static UnsafeTypeBinding FromContainer(
            this UnsafeTypeBinding typeBinding
        )
        {
            typeBinding.CreateConcreteDelegate = c => c.Resolve(typeBinding.ConcreteType);
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
        public static UnsafeTypeBinding FromContainer(
            this UnsafeTypeBinding typeBinding,
            Action<ResolutionConstraints> constraints
        )
        {
            typeBinding.CreateConcreteDelegate = c => c.Resolve(typeBinding.ConcreteType, constraints);
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
        public static UnsafeTypeBinding FromInstance(
            this UnsafeTypeBinding typeBinding,
            object instance
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
        public static UnsafeTypeBinding FromMethod(
            this UnsafeTypeBinding typeBinding,
            CreateDelegate createDelegate
        )
        {
            typeBinding.CreateConcreteDelegate = createDelegate;
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Lazy<TBinding>(this TBinding typeBinding)
            where TBinding : TypeBinding
        {
            typeBinding.IsLazy = true;
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding NonLazy<TBinding>(this TBinding typeBinding)
            where TBinding : TypeBinding
        {
            typeBinding.IsLazy = false;
            return typeBinding;
        }
    }
}
