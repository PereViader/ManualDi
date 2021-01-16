using System;

namespace ManualDI.TypeInjections
{
    public class ActionTypeInjection<T> : ITypeInjection<T>
    {
        public Action<T, IDiContainer> Action { get; }

        public ActionTypeInjection(Action<T, IDiContainer> action)
        {
            Action = action;
        }

        public void Inject(T instance, IDiContainer container)
        {
            Action.Invoke(instance, container);
        }
    }
}