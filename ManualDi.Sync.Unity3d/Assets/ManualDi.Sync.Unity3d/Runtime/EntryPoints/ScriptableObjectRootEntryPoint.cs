using System;
using UnityEngine;

namespace ManualDi.Sync.Unity3d
{
    public abstract class ScriptableObjectRootEntryPoint : ScriptableObject, IInstaller, IDisposable
    {
        public bool IsInitialized => Container is not null;
        public IDiContainer? Container { get; private set; }

        public void Initiate()
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Context is already initialized");
            }
            
            Container = new DiContainerBindings()
                .Install(this)
                .Build();
        }

        public void Dispose()
        {
            if (Container is null)
            {
                return;
            }

            Container.Dispose();
            Container = null;
        }

        public abstract void Install(DiContainerBindings b);
    }
}