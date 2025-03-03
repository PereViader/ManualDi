using System;

namespace ManualDi.Main
{
    public sealed class TypeBindingSync<TApparent, TConcrete> : TypeBinding, IInitializeBinding
    {
        public override Type ApparentType => typeof(TApparent);
        public override Type ConcreteType => typeof(TConcrete);

        public CreateDelegate<TConcrete>? CreateDelegate;
        public InstanceContainerDelegate<TConcrete>? InjectionDelegate;
        public InstanceContainerDelegate<TConcrete>? InitializationDelegate;

        internal override object Resolve(DiContainer diContainer)
        {
            if (SingleInstance is not null) //Optimization: We don't check if Scope is Single
            {
                return SingleInstance;
            }
            
            var previousInjectedTypeBinding = diContainer.injectedTypeBinding;
            diContainer.injectedTypeBinding = this;

            var instance = CreateDelegate!.Invoke(diContainer) //Optimization: Assumes it will be initialized
                           ?? throw new InvalidOperationException($"Could not create object for TypeBinding with Apparent type {typeof(TApparent)} and Concrete type {typeof(TConcrete)}");

            if (TypeScope is TypeScope.Single)
            {
                SingleInstance = instance;
            }
            
            InjectionDelegate?.Invoke((TConcrete)instance, diContainer);
            if (InitializationDelegate is not null)
            {
                diContainer.diContainerInitializer.QueueInitialize(this, instance);
            }
            
            if (TryToDispose && instance is IDisposable disposable)
            {
                diContainer.QueueDispose(disposable);
            }

            diContainer.injectedTypeBinding = previousInjectedTypeBinding;
            if (diContainer.injectedTypeBinding is null)
            {
                diContainer.diContainerInitializer.InitializeCurrentLevelQueued(diContainer);
            }

            return instance;
        }

        void IInitializeBinding.InitializeObject(object instance, DiContainer diContainer)
        {
            //Must only be used when not null, optimized for faster runtime
            InitializationDelegate!.Invoke((TConcrete)instance, diContainer);
        }
    }
}