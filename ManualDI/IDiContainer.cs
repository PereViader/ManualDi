using System;

namespace ManualDI
{
    public interface IDiContainer
    {
        void Bind<T>(Action<ITypeBinding<T>> typeSetup);
        T Resolve<T>();
    }
}