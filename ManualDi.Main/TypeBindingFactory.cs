using ManualDi.Main.TypeScopes;

namespace ManualDi.Main
{
    public class TypeBindingFactory : ITypeBindingFactory
    {
        public ITypeBinding<T> Create<T>(RegisterDisposeDelegate registerDisposeDelegate)
        {
            return new TypeBinding<T>(registerDisposeDelegate)
            {
                TypeScope = SingleTypeScope.Instance,
                TypeMetadata = new TypeMetadata(),
                IsLazy = true
            };
        }
    }
}
