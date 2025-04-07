using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    //In order to optimize the container, this is a struct that is modified by static ref extensions
    internal struct DiContainerDisposer
    {
        public readonly List<IDisposable> Disposables;
        public bool DisposedValue;

        public DiContainerDisposer(int? disposablesCount = null)
        {
            Disposables = disposablesCount.HasValue ? new(disposablesCount.Value) : new();
            DisposedValue = false;
        }
    }

    internal static class DiContainerDisposerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueDispose(ref this DiContainerDisposer o, IDisposable disposable)
        {
            o.Disposables.Add(disposable);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueDispose(ref this DiContainerDisposer o, Action disposableAction)
        {
            o.Disposables.Add(new ActionDisposableWrapper(disposableAction));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dispose(ref this DiContainerDisposer o)
        {
            if (o.DisposedValue)
            {
                return;
            }
            o.DisposedValue = true;
            
            foreach (var disposable in o.Disposables)
            {
                disposable.Dispose();
            }

            o.Disposables.Clear();
        }
    }
}
