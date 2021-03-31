namespace ManualDI
{
    public static class ConstraintExtensions
    {
        public static ITypeBinding<T> Metadata<T, Y>(this ITypeBinding<T> typeBinding, object key, Y value)
        {
            typeBinding.TypeMetadata.Set(key, value);
            return typeBinding;
        }

        public static ITypeBinding<T> Metadata<T>(this ITypeBinding<T> typeBinding, object key)
        {
            typeBinding.TypeMetadata.Set<object>(key, null);
            return typeBinding;
        }
    }
}
