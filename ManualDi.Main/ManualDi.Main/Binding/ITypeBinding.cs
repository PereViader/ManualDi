using System;
using System.Collections.Generic;

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
    
    public interface TypeBinding
    {
        public Type InterfaceType { get; }
        public Type ConcreteType { get; }
        public TypeScope TypeScope { get; set; }
        public bool IsLazy { get; }
        public bool TryToDispose { get; }
        public Dictionary<object, object>? Metadata { get; }

        public object Create(IDiContainer container);
        
        public bool NeedsInitialize { get; }
        public void InitializeObject(object instance, IDiContainer container);
        public void InjectObject(object instance, IDiContainer container);
    }
}
