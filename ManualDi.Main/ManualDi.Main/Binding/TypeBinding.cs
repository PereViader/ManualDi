using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ManualDi.Main
{
    public sealed class TypeBinding<TInterface, TConcrete> : TypeBinding
    {
        public Type InterfaceType => typeof(TInterface);
        public Type ConcreteType => typeof(TConcrete);
        public CreateDelegate<TConcrete>? CreateConcreteDelegate { get; set; }
        public CreateDelegate<TInterface>? CreateInterfaceDelegate { get; set; }
        public InjectionDelegate<TConcrete>? InjectionDelegates { get; set; }
        public InitializationDelegate<TConcrete>? InitializationDelegate { get; set; }
        public TypeScope TypeScope { get; set; } = TypeScope.Single;
        public bool IsLazy { get; set; } = true;
        public bool TryToDispose { get; set; } = true;
        public Dictionary<object, object>? Metadata { get; set; }
        
        public object Create(IDiContainer container)
        {
            if (TryCreate(container, out var invoke))
            {
                return invoke;
            }

            throw new InvalidOperationException(
                $"Could not create object for TypeBinding with apparent type {typeof(TInterface).FullName} and concrete type {typeof(TConcrete).FullName}");
        }

        public bool NeedsInitialize => InitializationDelegate is not null;

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

        public void InjectObject(object instance, IDiContainer container)
        {
            InjectionDelegates?.Invoke((TConcrete)instance, container);
        }
        
        public void InitializeObject(object instance, IDiContainer container)
        {
            InitializationDelegate?.Invoke((TConcrete)instance, container);
        }
    }
}
