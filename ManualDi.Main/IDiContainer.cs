using System;
using System.Collections.Generic;

namespace ManualDi.Main
{
    public interface IDiContainer
    {
        void Bind<T>(Action<ITypeBinding<T>> typeSetup);

        T Resolve<T>();
        T Resolve<T>(Action<IResolutionConstraints> resolution);

        bool TryResolve<T>(out T result);
        bool TryResolve<T>(Action<IResolutionConstraints> resolution, out T result);

        List<T> ResolveAll<T>();
        List<T> ResolveAll<T>(Action<IResolutionConstraints> resolution);

        void FinishBinding();
    }
}
