using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    public static class BindingDisposableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding SkipDisposable<TBinding>(this TBinding binding)
            where TBinding : Binding
        {
            binding.TryToDispose = false;
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Dispose<TBinding>(this TBinding binding, DisposeObjectDelegate disposeObjectDelegate) 
            where TBinding : Binding
        {
            binding.Inject((o, c) =>
            {
                c.QueueDispose(() => disposeObjectDelegate.Invoke(o));
            });
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Dispose<TBinding>(this TBinding binding, DisposeObjectContextDelegate disposeObjectContextDelegate)
            where TBinding : Binding
        {
            binding.Inject((o, c) =>
            {
                c.QueueDispose(disposeObjectContextDelegate.Invoke(o, c));
            });
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding DisposeAsync<TBinding>(this TBinding binding, AsyncDisposeObjectDelegate asyncDisposeObjectDelegate)
            where TBinding : Binding
        {
            binding.Inject((o, c) =>
            {
                c.QueueAsyncDispose(async () => await asyncDisposeObjectDelegate.Invoke(o));
            });
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding DisposeAsync<TBinding>(this TBinding binding, AsyncDisposeObjectContextDelegate asyncDisposeObjectContextDelegate)
            where TBinding : Binding
        {
            binding.Inject((o, c) =>
            {
                c.QueueAsyncDispose(asyncDisposeObjectContextDelegate.Invoke(o, c));
            });
            return binding;
        }
    }
}
