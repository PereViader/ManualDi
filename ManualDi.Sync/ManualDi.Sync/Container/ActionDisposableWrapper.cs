using System;

namespace ManualDi.Sync
{
    internal sealed class ActionDisposableWrapper : IDisposable
    {
        private Action? action;

        public ActionDisposableWrapper(Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            if (action is null)
            {
                return;
            }
            
            action.Invoke();
            action = null;
        }
    }
}