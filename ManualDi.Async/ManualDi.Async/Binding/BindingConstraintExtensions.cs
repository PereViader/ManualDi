using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    public static class BindingConstraintExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding WithId<TBinding>(this TBinding binding, object id)
            where TBinding : Binding
        {
            binding.Id = id;
            return binding;
        }
    }
}
