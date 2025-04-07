using System;
using UnityEngine;

namespace ManualDi.Sync.Unity3d
{
    public abstract class ScriptableObjectSubordinateEntryPoint<TData> : ScriptableObject, IInstaller, IDisposable
    {
        public bool IsInitialized => Container is not null;
        public IDiContainer? Container { get; private set; }
        public TData? Data { get; private set; }
        
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
    
    public abstract class ScriptableObjectSubordinateEntryPoint<TData, TContext> : ScriptableObject, IInstaller, IDisposable
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