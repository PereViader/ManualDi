using System;

namespace ManualDi.Main
{
    public delegate T? CreateDelegate<out T>(IDiContainer diContainer);
    public delegate void InstanceContainerDelegate<in T>(T instance, IDiContainer diContainer);
    public delegate bool FilterBindingDelegate(BindingContext context);

    public sealed class BindingContext
    {
        public TypeBinding TypeBinding = default!;
        public TypeBinding? InjectedIntoTypeBinding;
    }

    public enum TypeScope
    {
        Single,
        Transient
    }
    
    internal interface IInitializeBinding
    {
        void InitializeObject(object instance, DiContainer diContainer);
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

        public abstract object? ResolveBinding(DiContainer diContainer);
    }
    
    public sealed class TypeBinding<TApparent, TConcrete> : TypeBinding, IInitializeBinding
    {
        public override Type ApparentType => typeof(TApparent);
        public override Type ConcreteType => typeof(TConcrete);

        public CreateDelegate<TConcrete>? CreateConcreteDelegate;
        public InstanceContainerDelegate<TConcrete>? InjectionDelegate;
        public InstanceContainerDelegate<TConcrete>? InitializationDelegate;
        
        public override object? ResolveBinding(DiContainer diContainer)
        {
            return diContainer.ResolveBinding(this);
        }

        public void InitializeObject(object instance, DiContainer diContainer)
        {
            //Must only be used when not null, optimized for faster runtime
            InitializationDelegate!.Invoke((TConcrete)instance, diContainer);
        }
    }
    
    // Attention Consider any methods that use this experimental. They may be removed in the future.
    public sealed class UnsafeTypeBinding : TypeBinding, IInitializeBinding
    {
        public UnsafeTypeBinding(Type apparentType, Type concreteType)
        {
            ApparentType = apparentType;
            ConcreteType = concreteType;
        }

        public override Type ApparentType { get; }
        public override Type ConcreteType { get; }
        
        public CreateDelegate<object>? CreateConcreteDelegate;
        public InstanceContainerDelegate<object>? InjectionDelegate;
        public InstanceContainerDelegate<object>? InitializationDelegate;
        
        public override object? ResolveBinding(DiContainer diContainer)
        {
            return diContainer.ResolveBinding(this);
        }
        
        public void InitializeObject(object instance, DiContainer diContainer)
        {
            //Must only be used when not null, optimized for faster runtime
            InitializationDelegate!.Invoke(instance, diContainer);
        }
    }
}
