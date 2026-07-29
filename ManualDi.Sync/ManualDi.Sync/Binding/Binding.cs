using System;

namespace ManualDi.Sync
{
    public delegate object? FromDelegate(IDiContainer diContainer);
    public delegate void InstanceContainerDelegate(object instance, IDiContainer diContainer);

    public abstract class Binding
    {
        public abstract Type ConcreteType { get; }
        
        public bool TryToDispose = true;
        public object? FromDelegate; // Contains either FromDelegate or an instance of type TConcrete
        public InstanceContainerDelegate? InjectionDelegate;
        public InstanceContainerDelegate? InitializationDelegate;

        public bool IsTransient; // Binding is either transient or single
        internal object? Instance;
    }

    public sealed class Binding<TConcrete> : Binding
    {
        public override Type ConcreteType => typeof(TConcrete);
    }
}
