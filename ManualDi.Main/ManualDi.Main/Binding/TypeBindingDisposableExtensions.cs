using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingDisposableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding DontDispose<TBinding>(this TBinding typeBinding)
            where TBinding : TypeBinding
        {
            typeBinding.TryToDispose = false;
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> Dispose<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            DisposeObjectDelegate<TConcrete> disposeObjectDelegate
            )
        {
            typeBindingSync.Inject((o, c) =>
            {
                c.QueueDispose(() => disposeObjectDelegate.Invoke(o));
            });
            return typeBindingSync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> Dispose<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            DisposeObjectContextDelegate<TConcrete> disposeObjectContextDelegate
        )
        {
            typeBindingSync.Inject((o, c) =>
            {
                c.QueueDispose(disposeObjectContextDelegate.Invoke(o, c));
            });
            return typeBindingSync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> Dispose<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            DisposeObjectDelegate<TConcrete> disposeObjectDelegate
        )
        {
            typeBindingAsync.Inject((o, c) =>
            {
                c.QueueDispose(() => disposeObjectDelegate.Invoke(o));
            });
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> Dispose<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            DisposeObjectContextDelegate<TConcrete> disposeObjectContextDelegate
        )
        {
            typeBindingAsync.Inject((o, c) =>
            {
                c.QueueDispose(disposeObjectContextDelegate.Invoke(o, c));
            });
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> DisposeAsync<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            AsyncDisposeObjectDelegate<TConcrete> asyncDisposeObjectDelegate
        )
        {
            typeBindingAsync.Inject((o, c) =>
            {
                c.QueueAsyncDispose(async () => await asyncDisposeObjectDelegate.Invoke(o));
            });
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> DisposeAsync<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            AsyncDisposeObjectContextDelegate<TConcrete> asyncDisposeObjectContextDelegate
        )
        {
            typeBindingAsync.Inject((o, c) =>
            {
                c.QueueAsyncDispose(asyncDisposeObjectContextDelegate.Invoke(o, c));
            });
            return typeBindingAsync;
        }
    }
}
