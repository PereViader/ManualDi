using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ManualDi.Async.Unity3d
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
        
        /// <summary>
        /// Override this method to add behaviour that should happen before and after the container is setup
        /// </summary>
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