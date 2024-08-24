using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public delegate T? CreateDelegate<out T>(IDiContainer diContainer);
    public delegate void InitializationDelegate<in T>(T instance, IDiContainer container);
    public delegate void InjectionDelegate<in T>(T instance, IDiContainer diContainer);

    public enum TypeScope
    {
        Single,
        Transient
    }
    
    public abstract class TypeBinding
    {
        public abstract Type InterfaceType { get; }
        public abstract Type ConcreteType { get; }

        public TypeScope TypeScope { get; set; } = TypeScope.Single;
        public bool IsLazy { get; set; } = true;
        public bool TryToDispose { get; set; } = true;
        public Dictionary<object, object>? Metadata { get; set; }
        
        internal object? SingleInstance { get; set; }

        public abstract (object instance, bool isNew) Create(IDiContainer container);
        
        public abstract bool NeedsInitialize { get; }
        public abstract void InitializeObject(object instance, IDiContainer container);
        public abstract void InjectObject(object instance, IDiContainer container);
    }
    
    public sealed class TypeBinding<TInterface, TConcrete> : TypeBinding
    {
        public override Type InterfaceType => typeof(TInterface);
        public override Type ConcreteType => typeof(TConcrete);
        public CreateDelegate<TConcrete>? CreateConcreteDelegate { get; set; }
        public InjectionDelegate<TConcrete>? InjectionDelegates { get; set; }
        public InitializationDelegate<TConcrete>? InitializationDelegate { get; set; }
        public override bool NeedsInitialize => InitializationDelegate is not null;
        
        public override (object instance, bool isNew) Create(IDiContainer container)
        {
            var result = TryCreate(container);
            if (result.HasValue)
            {
                return result.Value;
            }

            throw new InvalidOperationException(
                $"Could not create object for TypeBinding with apparent type {typeof(TInterface).FullName} and concrete type {typeof(TConcrete).FullName}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (object instance, bool isNew)? TryCreate(IDiContainer container)
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
                    return null;
                }
                
                return (SingleInstance, true);
            }
            
            var instance = CreateNew(container);
            if (instance is null)
            {
                return null;
            }
            return (instance, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object? CreateNew(IDiContainer container)
        {
            if (CreateConcreteDelegate is not null)
            {
                return CreateConcreteDelegate.Invoke(container);
            }

            return null;
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
}
