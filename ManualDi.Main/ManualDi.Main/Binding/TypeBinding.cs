using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public delegate T? CreateDelegate<out T>(IDiContainer diContainer);
    public delegate void InstanceContainerDelegate<in T>(T instance, IDiContainer diContainer);
    

    public enum TypeScope
    {
        Single,
        Transient
    }
    
    public abstract class TypeBinding
    {
        public TypeScope TypeScope { get; set; } = TypeScope.Single;
        public bool IsLazy { get; set; } = true;
        public bool TryToDispose { get; set; } = true;
        public Dictionary<object, object>? Metadata { get; set; }
        
        internal object? SingleInstance { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (object instance, bool isNew) Create(IDiContainer container)
        {
            if (TypeScope is TypeScope.Single)
            {
                if (SingleInstance is not null)
                {
                    return (SingleInstance, false);
                }

                SingleInstance = CreateNew(container);
                if (SingleInstance is null)
                {
                    throw new InvalidOperationException($"Could not create object for {GetType().FullName}");
                }
                
                return (SingleInstance, true);
            }
            
            var instance = CreateNew(container);
            if (instance is null)
            {
                throw new InvalidOperationException($"Could not create object for {GetType().FullName}");
            }
            return (instance, true);
        }
        
        protected abstract object? CreateNew(IDiContainer container);
        public abstract bool NeedsInitialize { get; }
        public abstract void InitializeObject(object instance, IDiContainer container);
        public abstract void InjectObject(object instance, IDiContainer container);
    }
    
    public sealed class TypeBinding<TInterface, TConcrete> : TypeBinding
    {
        public CreateDelegate<TConcrete>? CreateConcreteDelegate { get; set; }
        public InstanceContainerDelegate<TConcrete>? InjectionDelegates { get; set; }
        public InstanceContainerDelegate<TConcrete>? InitializationDelegate { get; set; }
        public override bool NeedsInitialize => InitializationDelegate is not null;

        protected override object? CreateNew(IDiContainer container)
        {
            if (CreateConcreteDelegate is null)
            {
                return null;
            }

            return CreateConcreteDelegate.Invoke(container);
        }

        public override void InjectObject(object instance, IDiContainer container)
        {
            InjectionDelegates?.Invoke((TConcrete)instance, container);
        }
        
        public override void InitializeObject(object instance, IDiContainer container)
        {
            InitializationDelegate?.Invoke((TConcrete)instance, container);
        }
    }
    
    // Attention Consider any methods that use this experimental. They may be removed in the future.
    public sealed class UnsafeTypeBinding : TypeBinding
    {
        public UnsafeTypeBinding(Type interfaceType, Type concreteType)
        {
            InterfaceType = interfaceType;
            ConcreteType = concreteType;
        }

        public Type InterfaceType { get; }
        public Type ConcreteType { get; }
        public CreateDelegate<object>? CreateConcreteDelegate { get; set; }
        public InstanceContainerDelegate<object>? InjectionDelegates { get; set; }
        public InstanceContainerDelegate<object>? InitializationDelegate { get; set; }
        public override bool NeedsInitialize => InitializationDelegate is not null;
        
        protected override object? CreateNew(IDiContainer container)
        {
            return CreateConcreteDelegate?.Invoke(container);
        }

        public override void InjectObject(object instance, IDiContainer container)
        {
            InjectionDelegates?.Invoke(instance, container);
        }
        
        public override void InitializeObject(object instance, IDiContainer container)
        {
            InitializationDelegate?.Invoke(instance, container);
        }
    }
}
