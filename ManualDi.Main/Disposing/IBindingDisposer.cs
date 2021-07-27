using System;

namespace ManualDi.Main.Disposing
{
    public interface IBindingDisposer
    {
        void RegisterDispose(Action disposeAction);
        void DisposeAll();
    }
}
