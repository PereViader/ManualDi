namespace ManualDi.Main
{
    public static class ConstraintExtensions
    {
        public static ITypeBinding<T> WithMetadata<T, Y>(this ITypeBinding<T> typeBinding, object key, Y value)
        {
            typeBinding.TypeMetadata.Set(key, value);
            return typeBinding;
        }

        public static ITypeBinding<T> WithMetadata<T>(this ITypeBinding<T> typeBinding, object key)
        {
            typeBinding.TypeMetadata.Set<object>(key, null);
            return typeBinding;
        }
    }
}
