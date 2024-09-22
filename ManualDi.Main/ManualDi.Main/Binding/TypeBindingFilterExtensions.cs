using System.Runtime.CompilerServices;

namespace ManualDi.Main
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