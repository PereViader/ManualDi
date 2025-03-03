using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    //In order to optimize the container, this is a struct that is modified by static ref extensions
    internal struct DiContainerDisposer
    {
        public readonly List<IDisposable> Disposables;
        public readonly List<IAsyncDisposable> AsyncDisposables;
        public bool DisposedValue;

        public DiContainerDisposer(int? disposablesCount = null, int? asyncDisposablesCount = null)
        {
            Disposables = disposablesCount.HasValue ? new(disposablesCount.Value) : new();
            AsyncDisposables = asyncDisposablesCount.HasValue ? new(asyncDisposablesCount.Value) : new();
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
        public static void QueueAsyncDispose(ref this DiContainerDisposer o, IAsyncDisposable asyncDisposable)
        {
            o.AsyncDisposables.Add(asyncDisposable);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueAsyncDispose(ref this DiContainerDisposer o, Func<ValueTask> asyncDisposable)
        {
            o.AsyncDisposables.Add(new FuncAsyncDisposableWrapper(asyncDisposable));
        }
    }
}
