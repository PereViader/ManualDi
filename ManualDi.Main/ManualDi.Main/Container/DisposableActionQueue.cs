using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    //In order to optimize the container, this is a struct that is modified by static ref extensions
    internal struct DisposableActionQueue
    {
        public readonly List<IDisposable> Disposables;
        public bool IsDisposing;

        public DisposableActionQueue(int? disposablesCount = null)
        {
            Disposables = disposablesCount.HasValue ? new(disposablesCount.Value) : new();
            IsDisposing = false;
        }
    }

    internal static class DisposableActionQueueExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueDispose(ref this DisposableActionQueue o, IDisposable disposable)
        {
            if (o.IsDisposing)
            {
                throw new InvalidOperationException(
                    "Tried to register a dispose action while disposing"
                );
            }

            o.Disposables.Add(disposable);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueDispose(ref this DisposableActionQueue o, Action disposableAction)
        {
            o.QueueDispose(new ActionDisposableWrapper(disposableAction));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeAll(ref this DisposableActionQueue o)
        {
            o.IsDisposing = true;

            foreach (var disposable in o.Disposables)
            {
                disposable.Dispose();
            }

            o.Disposables.Clear();
            
            o.IsDisposing = false;
        }
    }
}
