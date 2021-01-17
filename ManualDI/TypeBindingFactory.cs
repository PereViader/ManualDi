using ManualDI.TypeScopes;

namespace ManualDI
{
    public class TypeBindingFactory : ITypeBindingFactory
    {
        public ITypeBinding<T> Create<T>()
        {
            return new TypeBinding<T>()
            {
                TypeScope = SingleTypeScope.Instance
            };
        }
    }
}
