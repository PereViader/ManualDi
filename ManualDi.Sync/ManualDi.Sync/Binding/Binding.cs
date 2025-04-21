using System;

namespace ManualDi.Sync
{
    public delegate T? CreateDelegate<out T>(IDiContainer diContainer);
    public delegate void InstanceContainerDelegate<in T>(T instance, IDiContainer diContainer);
    public delegate bool FilterBindingDelegate(BindingContext context);

    public sealed class BindingContext
    {
        public Binding Binding = default!;
        public Binding? InjectedIntoBinding;
    }
    
    internal interface IInitializeBinding
    {
        void InitializeObject(object instance, DiContainer diContainer);
    }
    
    public abstract class Binding
    {
        public abstract Type ApparentType { get; }
        public abstract Type ConcreteType { get; }
        
        public bool TryToDispose = true;
        public object? Id;
        public FilterBindingDelegate? FilterBindingDelegate;
        internal object? Instance;
        internal Binding? NextBinding;

        internal abstract object? Create(DiContainer diContainer);
        internal abstract bool Inject(DiContainer diContainer, object instance);
    }
    
    public sealed class Binding<TApparent, TConcrete> : Binding, IInitializeBinding
    {
        public override Type ApparentType => typeof(TApparent);
        public override Type ConcreteType => typeof(TConcrete);

        public CreateDelegate<TConcrete>? CreateConcreteDelegate;
        public InstanceContainerDelegate<TConcrete>? InjectionDelegate;
        public InstanceContainerDelegate<TConcrete>? InitializationDelegate;
        
        internal override object? Create(DiContainer diContainer)
        {
            return CreateConcreteDelegate!.Invoke(diContainer);
        }

        internal override bool Inject(DiContainer diContainer, object instance)
        {
            InjectionDelegate?.Invoke((TConcrete)instance, diContainer);
            return InitializationDelegate is not null;
        }

        void IInitializeBinding.InitializeObject(object instance, DiContainer diContainer)
        {
            //Must only be used when not null, optimized for faster runtime
            InitializationDelegate!.Invoke((TConcrete)instance, diContainer);
        }
    }
}
