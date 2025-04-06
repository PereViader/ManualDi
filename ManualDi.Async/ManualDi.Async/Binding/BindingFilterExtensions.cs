using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    public static class BindingFilterExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding When<TBinding>(this TBinding binding, FilterBindingDelegate filterBindingDelegate)
            where TBinding : Binding
        {            
            var previous = binding.FilterBindingDelegate;
            binding.FilterBindingDelegate = previous is null
                ? filterBindingDelegate
                : x => previous.Invoke(x) && filterBindingDelegate.Invoke(x);

            return binding;
        }
    }
}