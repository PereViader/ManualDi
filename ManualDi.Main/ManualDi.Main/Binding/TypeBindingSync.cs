using System;

namespace ManualDi.Main
{
    internal interface ITypeBindingSyncSetup
    {
        void Create(DiContainer diContainer);
        void Inject(DiContainer diContainer);
        void Initialize();
    }
    
    public class TypeBindingSync<TApparent, TConcrete> : TypeBinding, ITypeBindingSyncSetup
    {
        public override Type ConcreteType => typeof(TConcrete);

        public FromDelegate<TConcrete>? CreateDelegate;
        public InjectDelegate<TConcrete>? InjectionDelegate;
        public InitializeDelegate<TConcrete>? InitializationDelegate;

        void ITypeBindingSyncSetup.Create(DiContainer diContainer)
        {
            var instance = CreateDelegate!.Invoke(diContainer) //Optimization: Assumes it will be initialized
                         ?? throw new InvalidOperationException($"Could not create object for TypeBinding with Apparent type {typeof(TApparent)} and Concrete type {typeof(TConcrete)}");
            Instance = instance;
            
            if (TryToDispose)
            {
                switch (instance)
                {
                    case IAsyncDisposable asyncDisposable:
                        diContainer.QueueAsyncDispose(asyncDisposable);
                        break;
                    case IDisposable disposable:
                        diContainer.QueueDispose(disposable);
                        break;
                }
            }
        }
        
        void ITypeBindingSyncSetup.Inject(DiContainer diContainer)
        {
            InjectionDelegate?.Invoke((TConcrete)Instance!, diContainer);
        }

        void ITypeBindingSyncSetup.Initialize()
        {
            InitializationDelegate?.Invoke((TConcrete)Instance!);
        }
    }
}