using System;

namespace ManualDi.Sync
{
    public delegate object? FromDelegate(IDiContainer diContainer);
    public delegate void InstanceContainerDelegate(object instance, IDiContainer diContainer);
    public delegate bool FilterBindingDelegate(BindingContext context);

    public sealed class BindingContext
    {
        public Binding Binding = null!;
        public Binding? InjectedIntoBinding;
    }
    
    public abstract class Binding
    {
        public abstract Type ApparentType { get; }
        public abstract Type ConcreteType { get; }
        
        public bool TryToDispose = true;
        public FilterBindingDelegate? FilterBindingDelegate;
        public object? Id;
        public object? FromDelegate; // Contains either FromDelegate or an instance of type TConcrete
        public InstanceContainerDelegate? InjectionDelegate;
        public InstanceContainerDelegate? InitializationDelegate;
        
        internal Binding? NextBinding;
        internal object? Instance;
    }
    
    public sealed class Binding<TApparent, TConcrete> : Binding
    {
        public override Type ApparentType => typeof(TApparent);
        public override Type ConcreteType => typeof(TConcrete);
    }
}
