using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingFromExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> FromContainerResolve<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding
            )
        {
            typeBinding.CreateConcreteDelegate = c => c.Resolve<TConcrete>();
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeTypeBinding FromContainerResolve(
            this UnsafeTypeBinding typeBinding
        )
        {
            typeBinding.CreateConcreteDelegate = c => c.Resolve(typeBinding.ConcreteType);
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> FromContainerResolve<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            Action<ResolutionConstraints> constraints
            )
        {
            typeBinding.CreateConcreteDelegate = c => c.Resolve<TConcrete>(constraints);
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeTypeBinding FromContainerResolve(
            this UnsafeTypeBinding typeBinding,
            Action<ResolutionConstraints> constraints
        )
        {
            typeBinding.CreateConcreteDelegate = c => c.Resolve(typeBinding.ConcreteType, constraints);
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> FromSubContainerResolve<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            InstallDelegate installDelegate,
            bool isContainerParent = true
        )
        {
            IDiContainer? subContainer = null;
            typeBinding.CreateConcreteDelegate = c =>
            {
                var bindings = new DiContainerBindings().Install(installDelegate);
                if (isContainerParent)
                {
                    bindings.WithParentContainer(c);
                }
                subContainer = bindings.Build();
                return subContainer.Resolve<TConcrete>();
            };
            typeBinding.Dispose((o, c) => subContainer?.Dispose());
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeTypeBinding FromSubContainerResolve(
            this UnsafeTypeBinding typeBinding,
            InstallDelegate installDelegate,
            bool isContainerParent = true
        )
        {
            IDiContainer? subContainer = null;
            typeBinding.CreateConcreteDelegate = c =>
            {
                var bindings = new DiContainerBindings().Install(installDelegate);
                if (isContainerParent)
                {
                    bindings.WithParentContainer(c);
                }
                subContainer = bindings.Build();
                return subContainer.Resolve(typeBinding.ConcreteType);
            };
            typeBinding.Dispose((o, c) => subContainer?.Dispose());
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
            CreateDelegate<object> createDelegate
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
