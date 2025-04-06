using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class TypeBindingFilterExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding When<TBinding>(this TBinding typeBinding, FilterBindingDelegate filterBindingDelegate)
            where TBinding : TypeBinding
        {            
            var previous = typeBinding.FilterBindingDelegate;
            typeBinding.FilterBindingDelegate = previous is null
                ? filterBindingDelegate
                : x => previous.Invoke(x) && filterBindingDelegate.Invoke(x);

            return typeBinding;
        }
    }
}