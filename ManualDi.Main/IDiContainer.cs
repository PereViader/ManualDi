using System;
using System.Collections.Generic;

namespace ManualDi.Main
{
    public interface IDiContainer : IDisposable
    {
        void Bind<T>(Action<ITypeBinding<T>> typeSetup);

        T Resolve<T>();
        T Resolve<T>(IResolutionConstraints resolutionConstraints);

        List<T> ResolveAll<T>();
        List<T> ResolveAll<T>(IResolutionConstraints resolutionConstraints);


        void FinishBinding();
    }
}
