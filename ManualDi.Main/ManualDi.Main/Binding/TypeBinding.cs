using System;
using System.Runtime.CompilerServices;

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
        
        public TypeScope TypeScope { get; set; } = TypeScope.Single;
        public bool IsLazy { get; set; } = true;
        public bool TryToDispose { get; set; } = true;
        public object? Id { get; set; }
        public FilterBindingDelegate? FilterBindingDelegate { get; set; }
        
        private object? SingleInstance { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (object instance, bool isNew) Create(IDiContainer container)
        {
            if (SingleInstance is not null) //Optimization: We don't check if Scope is Single
            {
                return (SingleInstance, false);
            }
        
            var instance = CreateNew(container) ?? throw new InvalidOperationException($"Could not create object for {GetType().FullName}");
            if (TypeScope is TypeScope.Single)
            {
                SingleInstance = instance;
            }
        
            return (instance, true);
        }
        
        protected abstract object? CreateNew(IDiContainer container);
        public abstract bool NeedsInitialize { get; }
        public abstract void InitializeObject(object instance, IDiContainer container);
        public abstract void InjectObject(object instance, IDiContainer container);
    }
    
    public sealed class TypeBinding<TApparent, TConcrete> : TypeBinding
    {
        public override Type ApparentType => typeof(TApparent);
        public override Type ConcreteType => typeof(TConcrete);
        
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
        public UnsafeTypeBinding(Type apparentType, Type concreteType)
        {
            ApparentType = apparentType;
            ConcreteType = concreteType;
        }

        public override Type ApparentType { get; }
        public override Type ConcreteType { get; }
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
