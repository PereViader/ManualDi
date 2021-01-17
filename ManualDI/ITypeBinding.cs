using ManualDI.TypeFactories;
using ManualDI.TypeInjections;
using ManualDI.TypeScopes;

namespace ManualDI
{
    public interface ITypeBinding<T>
    {
        object Identifier { get; set; }
        ITypeScope TypeScope { get; set; }
        ITypeFactory<T> Factory { get; set; }
        ITypeInjection<T> TypeInjection { get; set; }
    }
}
