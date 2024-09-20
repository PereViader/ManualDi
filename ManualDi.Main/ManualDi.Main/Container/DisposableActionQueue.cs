using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    //In order to optimize the container, this is a struct that is modified by static ref extensions
    internal struct DisposableActionQueue
    {
        public readonly List<IDisposable> Disposables;

        public DisposableActionQueue(int? disposablesCount = null)
        {
            Disposables = disposablesCount.HasValue ? new(disposablesCount.Value) : new();
        }
    }

    internal static class DisposableActionQueueExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueDispose(ref this DisposableActionQueue o, IDisposable disposable)
        {
            o.Disposables.Add(disposable);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueDispose(ref this DisposableActionQueue o, Action disposableAction)
        {
            o.Disposables.Add(new ActionDisposableWrapper(disposableAction));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeAll(ref this DisposableActionQueue o)
        {
            foreach (var disposable in o.Disposables)
            {
                disposable.Dispose();
            }

            o.Disposables.Clear();
        }
    }
}
