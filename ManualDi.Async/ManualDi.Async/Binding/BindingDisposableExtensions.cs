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
        public static Binding<TApparent, TConcrete> Dispose<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            DisposeObjectDelegate disposeObjectDelegate
        )
        {
            binding.Inject((o, c) =>
            {
                c.QueueDispose(() => disposeObjectDelegate.Invoke(o));
            });
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> Dispose<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            DisposeObjectContextDelegate disposeObjectContextDelegate
        )
        {
            binding.Inject((o, c) =>
            {
                c.QueueDispose(disposeObjectContextDelegate.Invoke(o, c));
            });
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> DisposeAsync<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            AsyncDisposeObjectDelegate asyncDisposeObjectDelegate
        )
        {
            binding.Inject((o, c) =>
            {
                c.QueueAsyncDispose(async () => await asyncDisposeObjectDelegate.Invoke(o));
            });
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> DisposeAsync<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            AsyncDisposeObjectContextDelegate asyncDisposeObjectContextDelegate
        )
        {
            binding.Inject((o, c) =>
            {
                c.QueueAsyncDispose(asyncDisposeObjectContextDelegate.Invoke(o, c));
            });
            return binding;
        }
    }
}
