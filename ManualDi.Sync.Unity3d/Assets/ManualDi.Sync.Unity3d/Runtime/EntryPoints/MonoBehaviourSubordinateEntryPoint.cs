using System;
using UnityEngine;

namespace ManualDi.Sync.Unity3d
{
    public abstract class MonoBehaviourSubordinateEntryPoint<TData> : MonoBehaviour, IInstaller, IDisposable
    {
        public bool IsInitialized => Container is not null;
        public IDiContainer? Container { get; private set; }
        public TData? Data { get; private set; }
        
        private bool _disposed;
        
        public void Initiate(TData data)
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
            Container = bindings.Build();
        }

        public virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Container is null)
            {
                return;
            }

            Container.Dispose();
            Container = null;

            Data = default;
        }

        public abstract void Install(DiContainerBindings b);
    }
    
    public abstract class MonoBehaviourSubordinateEntryPoint<TData, TContext> : MonoBehaviour, IInstaller, IDisposable
    {
        public bool IsInitialized => Container is not null;
        public IDiContainer? Container { get; private set; }
        public TContext? Context { get; private set; }
        public TData? Data { get; private set; }
        
        public TContext Initiate(TData data)
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

            Container = bindings.Build();

            Context = Container.Resolve<TContext>();

            return Context;
        }

        public virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Container is null)
            {
                return;
            }

            Container.Dispose();
            Container = null;

            Data = default;
            Context = default;
        }

        public abstract void Install(DiContainerBindings b);
    }
}
