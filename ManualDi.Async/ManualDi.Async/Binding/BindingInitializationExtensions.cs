using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ManualDi.Async
{
    public static class BindingInitializationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Initialize<TBinding>(this TBinding binding, InitializeDelegate initializationDelegate)
            where TBinding : Binding
        {
            binding.InitializationDelegate = binding.InitializationDelegate is null 
                ? initializationDelegate 
                : binding.InitializationDelegate switch
                {
                    InitializeDelegate existingSync => existingSync + initializationDelegate,
                    InitializeAsyncDelegate existingAsync => (InitializeAsyncDelegate)(async (o, ct) =>
                    {
                        await existingAsync(o, ct);
                        initializationDelegate(o);
                    }),
                    _ => throw new InvalidOperationException()
                };
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding InitializeAsync<TBinding>(this TBinding binding, InitializeAsyncDelegate initializationAsyncDelegate)
            where TBinding : Binding
        {
            binding.InitializationDelegate = binding.InitializationDelegate is null 
                ? initializationAsyncDelegate 
                : binding.InitializationDelegate switch
                {
                    InitializeDelegate existingSync => (InitializeAsyncDelegate)(async (o, ct) =>
                    {
                        existingSync(o);
                        await initializationAsyncDelegate(o, ct);
                    }),
                    InitializeAsyncDelegate existingAsync => (InitializeAsyncDelegate)(async (o, ct) =>
                    {
                        await existingAsync(o, ct);
                        await initializationAsyncDelegate(o, ct);
                    }),
                    _ => throw new InvalidOperationException()
                };
            return binding;
        }
    }
}
