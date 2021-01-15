using System;

namespace ManualDI
{
    public class ActionTypeInjection<T> : ITypeInjection<T>
    {
        public Action<IContainer, T> Action { get; }

        public ActionTypeInjection(Action<IContainer, T> action)
        {
            Action = action;
        }

        public void Inject(IContainer container, T instance)
        {
            Action.Invoke(container, instance);
        }
    }
}