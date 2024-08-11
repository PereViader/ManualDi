using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

        public abstract object Create(IDiContainer container);
        
        public abstract bool NeedsInitialize { get; }
        public abstract void InitializeObject(object instance, IDiContainer container);
        public abstract void InjectObject(object instance, IDiContainer container);
    }
    
    public sealed class TypeBinding<TInterface, TConcrete> : TypeBinding
    {
        public override Type InterfaceType => typeof(TInterface);
        public override Type ConcreteType => typeof(TConcrete);
        public CreateDelegate<TConcrete>? CreateConcreteDelegate { get; set; }
        public CreateDelegate<TInterface>? CreateInterfaceDelegate { get; set; }
        public InjectionDelegate<TConcrete>? InjectionDelegates { get; set; }
        public InitializationDelegate<TConcrete>? InitializationDelegate { get; set; }
        
        public override object Create(IDiContainer container)
        {
            if (TryCreate(container, out var invoke))
            {
                return invoke;
            }

            throw new InvalidOperationException(
                $"Could not create object for TypeBinding with apparent type {typeof(TInterface).FullName} and concrete type {typeof(TConcrete).FullName}");
        }

        public override bool NeedsInitialize => InitializationDelegate is not null;

        private bool TryCreate(IDiContainer container, [MaybeNullWhen(false)] out object invoke)
        {
            if (CreateConcreteDelegate is not null)
            {
                invoke = CreateConcreteDelegate.Invoke(container);
                return invoke is not null;
            }

            if (CreateInterfaceDelegate is not null)
            {
                invoke = CreateInterfaceDelegate.Invoke(container);
                return invoke is not null;
            }

            invoke = false;
            return false;
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
