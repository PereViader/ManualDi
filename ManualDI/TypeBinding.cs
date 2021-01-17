using ManualDI.TypeFactories;
using ManualDI.TypeInjections;
using ManualDI.TypeScopes;

namespace ManualDI
{
    public class TypeBinding<T> : ITypeBinding<T>
    {
        public ITypeScope TypeScope { get; set; }
        public ITypeFactory<T> Factory { get; set; }
        public ITypeInjection<T> TypeInjection { get; set; }
    }
}
