using System;
using System.Collections.Generic;

namespace ManualDi.Main
{
    public interface IDiContainer : IDisposable
    {
        void Bind<T>(Action<ITypeBinding<T, T>> typeSetup);
        void Bind<T, Y>(Action<ITypeBinding<T, Y>> typeSetup);

        T Resolve<T>();
        T Resolve<T>(IResolutionConstraints resolutionConstraints);

        List<T> ResolveAll<T>();
        List<T> ResolveAll<T>(IResolutionConstraints resolutionConstraints);


        void FinishBinding();

        void QueueDispose(Action disposeAction);
    }
}
