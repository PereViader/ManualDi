using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingFilterExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding When<TBinding>(this TBinding typeBinding, Action<ResolutionConstraints> resolutionConstraintsDelegate)
            where TBinding : TypeBinding
        {
            var constraints = new ResolutionConstraints();
            resolutionConstraintsDelegate(constraints);
            var filterBindingDelegate = constraints.FilterBindingDelegate;
            if (filterBindingDelegate is null)
            {
                return typeBinding;
            }
            
            var previous = typeBinding.FilterBindingDelegate;
            typeBinding.FilterBindingDelegate = previous is null
                ? filterBindingDelegate
                : x => previous.Invoke(x) && filterBindingDelegate.Invoke(x);

            return typeBinding;
        }
    }

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
        public static TypeBinding<TApparent, TConcrete> Dispose<TApparent, TConcrete>(
            this TypeBinding<TApparent, TConcrete> typeBinding,
            InstanceContainerDelegate<TConcrete> disposeDelegate
            )
        {
            typeBinding.Inject((o, c) => c.QueueDispose(() => disposeDelegate.Invoke(o, c)));
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeTypeBinding Dispose(
            this UnsafeTypeBinding typeBinding,
            InstanceContainerDelegate<object> disposeDelegate
            )
        {
            typeBinding.Inject((o, c) => c.QueueDispose(() => disposeDelegate.Invoke(o, c)));
            return typeBinding;
        }
    }
}
