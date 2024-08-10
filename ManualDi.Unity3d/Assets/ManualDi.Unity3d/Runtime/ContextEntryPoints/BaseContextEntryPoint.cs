using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d
{
    public abstract class BaseContextEntryPoint<TData, TContext> : MonoBehaviour, IContextEntryPoint<TData, TContext>, IInstaller
        where TContext : MonoBehaviour
    {
        private bool disposedValue;

        public IDiContainer? Container { get; private set; }

        public TContext? Context { get; private set; }
        public TData? Data { get; private set; }

        public GameObject GameObject => gameObject;

        public TContext Initiate(IDiContainer? parentDiContainer, TData data)
        {
            disposedValue = false;

            Data = data;

            Container = new DiContainerBuilder()
                .WithParentContainer(parentDiContainer)
                .Install(this)
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
            if (disposedValue)
            {
                return;
            }
            disposedValue = true;

            if (Container == null)
            {
                return;
            }

            Container.Dispose();
            Container = null;

            Data = default;
            Context = default;
        }

        public abstract void Install(DiContainerBindings bindings);
    }
}
