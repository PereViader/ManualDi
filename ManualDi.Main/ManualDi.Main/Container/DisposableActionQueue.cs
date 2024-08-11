﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    internal sealed class DisposableActionQueue
    {
        private readonly List<IDisposable> disposables = new();
        private bool disposing = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueDispose(IDisposable disposable)
        {
            if (disposing)
            {
                throw new InvalidOperationException(
                    "Tried to register a dispose action while disposing"
                    );
            }

            disposables.Add(disposable);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueDispose(Action disposableAction)
        {
            QueueDispose(new ActionDisposableWrapper(disposableAction));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DisposeAll()
        {
            disposing = true;

            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }

            disposing = false;

            disposables.Clear();
        }
    }
}
