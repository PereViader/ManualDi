using System;
using System.Threading;
using System.Threading.Tasks;
using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d
{
    public abstract class MonoBehaviourSubordinateEntryPoint<TData> : MonoBehaviour, IInstaller, IAsyncDisposable
    {
        public bool IsInitialized => Container is not null;
        public IDiContainer? Container { get; private set; }
        public TData? Data { get; private set; }
        
        public async ValueTask Initiate(TData data, CancellationToken ct)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Context is already initialized");
            }
            
            Data = data;

            var bindings = new DiContainerBindings();
            if (Data is IInstaller dataInstaller)
            {
                bindings.Install(dataInstaller);
            }
            bindings.Install(this);
            Container = await InitiateWrapper(bindings.Build(ct));
        }
        
        protected virtual ValueTask<DiContainer> InitiateWrapper(ValueTask<DiContainer> task)
        {
            return task;
        }

        public virtual async void OnDestroy()
        {
            await DisposeAsync();
        }

        public ValueTask DisposeAsync()
        {
            if (Container is null)
            {
                return default;
            }

            Data = default;

            var container = Container;
            Container = null;
            return container.DisposeAsync();
        }

        public abstract void Install(DiContainerBindings b);
    }
    
    public abstract class MonoBehaviourSubordinateEntryPoint<TData, TContext> : MonoBehaviour, IInstaller, IAsyncDisposable
    {
        public bool IsInitialized => Container is not null;
        public IDiContainer? Container { get; private set; }
        public TContext? Context { get; private set; }
        public TData? Data { get; private set; }
        
        public async ValueTask<TContext> Initiate(TData data, CancellationToken ct)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Context is already initialized");
            }
            
            Data = data;

            var bindings = new DiContainerBindings();
            if (Data is IInstaller dataInstaller)
            {
                bindings.Install(dataInstaller);
            }
            bindings.Install(this);

            Container = await InitiateWrapper(bindings.Build(ct));

            Context = Container.Resolve<TContext>();

            return Context;
        }
        
        protected virtual ValueTask<DiContainer> InitiateWrapper(ValueTask<DiContainer> task)
        {
            return task;
        }

        public virtual async void OnDestroy()
        {
            await DisposeAsync();
        }

        public ValueTask DisposeAsync()
        {
            if (Container is null)
            {
                return default;
            }

            Data = default;
            Context = default;

            var container = Container;
            Container = null;
            return container.DisposeAsync();
        }

        public abstract void Install(DiContainerBindings b);
    }
}
