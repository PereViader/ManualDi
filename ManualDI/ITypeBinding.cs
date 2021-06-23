using ManualDI.TypeFactories;
using ManualDI.TypeInjections;
using ManualDI.TypeScopes;

namespace ManualDI
{
    public interface ITypeBinding
    {
        ITypeMetadata TypeMetadata { get; set; }
        ITypeScope TypeScope { get; set; }
        ITypeFactory Factory { get; set; }
        ITypeInjection TypeInjection { get; set; }
        bool IsLazy { get; set; }
    }

    public interface ITypeBinding<T> : ITypeBinding
    {
    }
}
