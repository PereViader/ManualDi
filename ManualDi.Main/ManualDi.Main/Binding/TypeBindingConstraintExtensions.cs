using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingConstraintExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding WithId<TBinding>(this TBinding typeBinding, object id)
            where TBinding : TypeBinding
        {
            typeBinding.Id = id;
            return typeBinding;
        }
    }
}
