using System;
using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d
{
    public abstract class SubordinateEntryPoint<TData, TContext> : MonoBehaviour, IInstaller
        where TContext : MonoBehaviour
    {
        public bool IsInitialized => Container is not null;
        public IDiContainer? Container { get; private set; }
        public TContext? Context { get; private set; }
        public TData? Data { get; private set; }

        public GameObject GameObject => gameObject;

        public TContext Initiate(TData data, IDiContainer? parentDiContainer = null)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Context is already initialized");
            }
            
            Data = data;

            Container = new DiContainerBuilder()
                .WithParentContainer(parentDiContainer)
                .Install(b =>
                {
                    if (Data is IInstaller dataInstaller)
                    {
                        dataInstaller.Install(b);
                    }
                    Install(b);
                })
                .Build();

            Context = Container.Resolve<TContext>();

            return Context;
        }

        public void Teardown()
        {
            Dispose();
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
