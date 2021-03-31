using System;
using System.Collections.Generic;

namespace ManualDI
{
    public interface IDiContainer
    {
        void Bind<T>(Action<ITypeBinding<T>> typeSetup);

        T Resolve<T>();
        T Resolve<T>(Action<IResolutionConstraints> resolution);

        List<T> ResolveAll<T>();
        List<T> ResolveAll<T>(Action<IResolutionConstraints> resolution);
    }
}
