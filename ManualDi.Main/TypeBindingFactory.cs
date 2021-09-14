using ManualDi.Main.TypeScopes;

namespace ManualDi.Main
{
    public class TypeBindingFactory : ITypeBindingFactory
    {
        public ITypeBinding<TInterface, TConcrete> Create<TInterface, TConcrete>()
        {
            return new TypeBinding<TInterface, TConcrete>()
            {
                TypeScope = SingleTypeScope.Instance,
                TypeMetadata = new TypeMetadata(),
                IsLazy = true
            };
        }
    }
}
