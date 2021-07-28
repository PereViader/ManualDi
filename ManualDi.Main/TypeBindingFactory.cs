using ManualDi.Main.TypeScopes;

namespace ManualDi.Main
{
    public class TypeBindingFactory : ITypeBindingFactory
    {
        public ITypeBinding<T> Create<T>()
        {
            return new TypeBinding<T>()
            {
                TypeScope = SingleTypeScope.Instance,
                TypeMetadata = new TypeMetadata(),
                IsLazy = true
            };
        }
    }
}
