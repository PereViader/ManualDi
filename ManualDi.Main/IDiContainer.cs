using System;
using System.Collections.Generic;

namespace ManualDi.Main
{
    public interface IDiContainer : IDisposable
    {
        T Resolve<T>();
        T Resolve<T>(IResolutionConstraints resolutionConstraints);

        List<T> ResolveAll<T>();
        List<T> ResolveAll<T>(IResolutionConstraints resolutionConstraints);

        void QueueDispose(Action disposeAction);
    }
}
