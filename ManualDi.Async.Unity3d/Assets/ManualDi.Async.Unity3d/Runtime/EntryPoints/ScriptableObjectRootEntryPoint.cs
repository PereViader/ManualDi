using System;
using System.Threading;
using System.Threading.Tasks;
using ManualDi.Async;
using UnityEngine;

namespace ManualDi.Unity3d
{
    public abstract class ScriptableObjectRootEntryPoint : ScriptableObject, IInstaller, IAsyncDisposable
    {
        public bool IsInitialized => Container is not null;
        public IDiContainer? Container { get; private set; }

        public async ValueTask Initiate(CancellationToken ct)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Context is already initialized");
            }
            
            Container = await InitiateWrapper(new DiContainerBindings()
                .Install(this)
                .Build(ct));
        }
        
        protected virtual ValueTask<DiContainer> InitiateWrapper(ValueTask<DiContainer> task)
        {
            return task;
        }

        public ValueTask DisposeAsync()
        {
            if (Container is null)
            {
                return default;
            }

            var container = Container;
            Container = null;
            return container.DisposeAsync();
        }

        public abstract void Install(DiContainerBindings b);
    }
}