using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ManualDi.Async.Unity3d
{
    public abstract class MonoBehaviourRootEntryPoint : MonoBehaviour, IInstaller, IAsyncDisposable
    {
        public bool InitializeOnStart = true;
        
        public bool IsInitialized => Container is not null;
        public IDiContainer? Container { get; private set; }
        
        private bool _disposed;
        
        public async void Start()
        {
            if (!InitializeOnStart)
            {
                return;
            }
            
            await Initiate(destroyCancellationToken);
        }

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

        public virtual async void OnDestroy()
        {
            await DisposeAsync();
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
        }

        public abstract void Install(DiContainerBindings b);
    }
}