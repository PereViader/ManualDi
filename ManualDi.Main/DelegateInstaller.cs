using System;
using System.Collections.Generic;

namespace ManualDi.Main
{
    public class DelegateInstaller : IInstaller
    {
        private readonly List<Action<IDiContainer>> containerDelegates = new List<Action<IDiContainer>>();

        public void Add(Action<IDiContainer> action)
        {
            containerDelegates.Add(action);
        }

        public void Add<T>(Action<ITypeBinding<T>> action)
        {
            void TypeBindingToContainerAdapter(IDiContainer container)
            {
                container.Bind<T>(action);
            }

            containerDelegates.Add(TypeBindingToContainerAdapter);
        }

        public void Install(IDiContainer container)
        {
            foreach (var containerDelegate in containerDelegates)
            {
                containerDelegate.Invoke(container);
            }
        }
    }
}
