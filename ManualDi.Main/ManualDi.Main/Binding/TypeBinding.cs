using System;

namespace ManualDi.Main
{
    public delegate T? CreateDelegate<out T>(IDiContainer diContainer);
    public delegate void InstanceContainerDelegate<in T>(T instance, IDiContainer diContainer);
    public delegate bool FilterBindingDelegate(BindingContext context);

    public sealed class BindingContext
    {
        public TypeBinding TypeBinding { get; set; } = default!;
        public TypeBinding? InjectedIntoTypeBinding { get; set; }
    }

    public enum TypeScope
    {
        Single,
        Transient
    }
    
    public abstract class TypeBinding
    {
        public abstract Type ApparentType { get; }
        public abstract Type ConcreteType { get; }
        
        public TypeScope TypeScope = TypeScope.Single;
        public bool IsLazy = true;
        public bool TryToDispose = true;
        public object? Id;
        public FilterBindingDelegate? FilterBindingDelegate;
        internal object? SingleInstance;
        internal TypeBinding? NextTypeBinding;
        
        public abstract object? CreateNew(IDiContainer container);
        public abstract bool InjectObject(object instance, IDiContainer container);
        public abstract void InitializeObject(object instance, IDiContainer container);
    }
    
    public sealed class TypeBinding<TApparent, TConcrete> : TypeBinding
    {
        public override Type ApparentType => typeof(TApparent);
        public override Type ConcreteType => typeof(TConcrete);

        public CreateDelegate<TConcrete>? CreateConcreteDelegate;
        public InstanceContainerDelegate<TConcrete>? InjectionDelegates;
        public InstanceContainerDelegate<TConcrete>? InitializationDelegate;
        
        public override object? CreateNew(IDiContainer container)
        {
            if (CreateConcreteDelegate is null)
            {
                return null;
            }

            return CreateConcreteDelegate.Invoke(container);
        }

        public override bool InjectObject(object instance, IDiContainer container)
        {
            InjectionDelegates?.Invoke((TConcrete)instance, container);
            return InitializationDelegate is not null;
        }
        
        public override void InitializeObject(object instance, IDiContainer container)
        {
            InitializationDelegate?.Invoke((TConcrete)instance, container);
        }
    }
    
    // Attention Consider any methods that use this experimental. They may be removed in the future.
    public sealed class UnsafeTypeBinding : TypeBinding
    {
        public UnsafeTypeBinding(Type apparentType, Type concreteType)
        {
            ApparentType = apparentType;
            ConcreteType = concreteType;
        }

        public override Type ApparentType { get; }
        public override Type ConcreteType { get; }
        
        public CreateDelegate<object>? CreateConcreteDelegate;
        public InstanceContainerDelegate<object>? InjectionDelegates;
        public InstanceContainerDelegate<object>? InitializationDelegate;
        
        public override object? CreateNew(IDiContainer container)
        {
            return CreateConcreteDelegate?.Invoke(container);
        }

        public override bool InjectObject(object instance, IDiContainer container)
        {
            InjectionDelegates?.Invoke(instance, container);
            return InitializationDelegate is not null;
        }
        
        public override void InitializeObject(object instance, IDiContainer container)
        {
            InitializationDelegate?.Invoke(instance, container);
        }
    }
}
