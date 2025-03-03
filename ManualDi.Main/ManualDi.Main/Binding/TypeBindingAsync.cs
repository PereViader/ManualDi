using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    public sealed class TypeBindingAsync<TApparent, TConcrete> : TypeBinding
    {
        public override Type ApparentType => typeof(Task<TApparent>);
        public override Type ConcreteType => typeof(Task<object?>);

        public CreateDelegate<TConcrete>? CreateDelegate;
        public CreateAsyncDelegate<TConcrete>? CreateAsyncDelegate;
        public InstanceContainerDelegate<TConcrete>? InjectionDelegate;
        public InstanceContainerAsyncDelegate<TConcrete>? InjectionAsyncDelegate;
        public InstanceContainerDelegate<TConcrete>? InitializationDelegate;
        public InstanceContainerAsyncDelegate<TConcrete>? InitializationAsyncDelegate;
        
        internal override object Resolve(DiContainer diContainer)
        {
            if (SingleInstance is not null) //Optimization: We don't check if Scope is Single
            {
                return SingleInstance;
            }
            
            var previousInjectedTypeBinding = diContainer.injectedTypeBinding;
            diContainer.injectedTypeBinding = this;

            var task = ResolveAsync(diContainer, diContainer.CancellationToken); 
            
            if (TypeScope is TypeScope.Single)
            {
                SingleInstance = task;
            }
            
            diContainer.injectedTypeBinding = previousInjectedTypeBinding;
            if (diContainer.injectedTypeBinding is null)
            {
                diContainer.diContainerInitializer.InitializeCurrentLevelQueued(diContainer);
            }
            
            return task;
        }
        
        private async Task<object?> ResolveAsync(DiContainer diContainer, CancellationToken ct)
        {
            var instance = (CreateDelegate, CreateAsyncDelegate) switch
            {
                (not null, not null) => throw new InvalidOperationException($"TypeBindingAsync with Apparent type {typeof(TApparent)} and Concrete type {typeof(TConcrete)} has both sync and async creation delegates. It should only have one."),
                (null, null) => throw new InvalidOperationException($"TypeBindingAsync with Apparent type {typeof(TApparent)} and Concrete type {typeof(TConcrete)} has no creation delegates defined."),
                (not null, null) => CreateDelegate.Invoke(diContainer) ?? throw new InvalidOperationException($"Could not create object for TypeBindingAsync with Apparent type {typeof(TApparent)} and Concrete type {typeof(TConcrete)}"),
                (null, not null) => await CreateAsyncDelegate.Invoke(diContainer, ct) ?? throw new InvalidOperationException($"Could not create object for TypeBindingAsync with Apparent type {typeof(TApparent)} and Concrete type {typeof(TConcrete)}"),
            };
            
            try
            {
                if (InjectionAsyncDelegate is not null)
                {
                    await InjectionAsyncDelegate.Invoke(instance, diContainer, ct);
                }
                InjectionDelegate?.Invoke(instance, diContainer);
                
                if (InitializationAsyncDelegate is not null)
                {
                    await InitializationAsyncDelegate.Invoke(instance, diContainer, ct);
                }
                InitializationDelegate?.Invoke(instance, diContainer);
            
                if (TryToDispose && instance is IDisposable disposable)
                {
                    diContainer.diContainerDisposer.QueueDispose(disposable);
                }
            
                if (TryToDispose && instance is IAsyncDisposable asyncDisposable)
                {
                    diContainer.diContainerDisposer.QueueAsyncDispose(asyncDisposable);
                }
            }
            finally
            {
                if (ct.IsCancellationRequested)
                {
                    if (instance is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync();   
                    }
                    else if (instance is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    
                    instance = default;
                }
            }
            
            return instance;
        }
    }
}