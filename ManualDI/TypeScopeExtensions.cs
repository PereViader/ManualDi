using ManualDi.TypeScopes;

namespace ManualDi
{
    public static class TypeScopeExtensions
    {
        public static ITypeBinding<T> Transient<T>(this ITypeBinding<T> typeBinding)
        {
            typeBinding.TypeScope = TransientTypeScope.Instance;
            return typeBinding;
        }

        public static ITypeBinding<T> Single<T>(this ITypeBinding<T> typeBinding)
        {
            typeBinding.TypeScope = SingleTypeScope.Instance;
            return typeBinding;
        }
    }
}
