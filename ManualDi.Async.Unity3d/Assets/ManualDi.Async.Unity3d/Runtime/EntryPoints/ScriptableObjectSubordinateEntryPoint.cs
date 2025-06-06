using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ManualDi.Async.Unity3d
{
    public abstract class ScriptableObjectSubordinateEntryPoint<TData> : ScriptableObject, IInstaller, IAsyncDisposable
    {
        public bool IsInitialized => Container is not null;
        public IDiContainer? Container { get; private set; }
        public TData? Data { get; private set; }
        
        private bool _disposed;
        
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
        
        /// <summary>
        /// Override this method to add behaviour that should happen before and after the container is setup
        /// </summary>
        protected virtual ValueTask<DiContainer> InitiateWrapper(ValueTask<DiContainer> task)
        {
            return task;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            if (Container is not null)
            {
                await Container.DisposeAsync();
            }
            Container = null;
            Data = default;
        }

        public abstract void Install(DiContainerBindings b);
    }
    
    public abstract class ScriptableObjectSubordinateEntryPoint<TData, TContext> : ScriptableObject, IInstaller, IAsyncDisposable
    {
        public bool IsInitialized => Container is not null;
        public IDiContainer? Container { get; private set; }
        public TContext? Context { get; private set; }
        public TData? Data { get; private set; }
        
        private bool _disposed;
        
        public async ValueTask<TContext> Initiate(TData data,  CancellationToken ct)
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

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            if (Container is not null)
            {
                await Container.DisposeAsync();
            }
            Container = null;
            Data = default;
            Context = default;
        }

        public abstract void Install(DiContainerBindings b);
    }
}