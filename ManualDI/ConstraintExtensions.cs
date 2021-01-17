namespace ManualDI
{
    public static class ConstraintExtensions
    {
        public static ITypeBinding<T> Id<T>(this ITypeBinding<T> typeBinding, object identifier)
        {
            typeBinding.Identifier = identifier;
            return typeBinding;
        }
    }
}
