using System;

namespace ManualDi.Main
{
    internal interface ITypeBindingSyncSetup
    {
        void CreateAndInject(DiContainer diContainer);
        void InitializeInstance(DiContainer diContainer);
    }
    
    public sealed class TypeBindingSync<TApparent, TConcrete> : TypeBinding, ITypeBindingSyncSetup
    {
        public override Type ApparentType => typeof(TApparent);
        public override Type ConcreteType => typeof(TConcrete);

        public FromDelegate<TConcrete>? CreateDelegate;
        public InjectDelegate<TConcrete>? InjectionDelegate;
        public InitializeDelegate<TConcrete>? InitializationDelegate;

        void ITypeBindingSyncSetup.CreateAndInject(DiContainer diContainer)
        {
            if (Instance is not null)
            {
                return;
            }
            
            var previousInjectedTypeBinding = diContainer.injectedTypeBinding;
            diContainer.injectedTypeBinding = this;

            var instance = CreateDelegate!.Invoke(diContainer) //Optimization: Assumes it will be initialized
                           ?? throw new InvalidOperationException($"Could not create object for TypeBinding with Apparent type {typeof(TApparent)} and Concrete type {typeof(TConcrete)}");
            Instance = instance;
            
            InjectionDelegate?.Invoke((TConcrete)instance, diContainer);
            if (InitializationDelegate is not null)
            {
                diContainer.diContainerInitializer.QueueInitialize(this);
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
        }

        void ITypeBindingSyncSetup.InitializeInstance(DiContainer diContainer)
        {
            //Must only be used when not null, optimized for faster runtime
            InitializationDelegate!.Invoke((TConcrete)Instance);
        }
    }
}