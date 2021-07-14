using ManualDI.TypeFactories;
using ManualDI.TypeInjections;
using ManualDI.TypeScopes;
using System.Collections.Generic;

namespace ManualDI
{
    public interface ITypeBinding
    {
        ITypeMetadata TypeMetadata { get; set; }
        ITypeScope TypeScope { get; set; }
        ITypeFactory Factory { get; set; }
        List<ITypeInjection> TypeInjections { get; set; }
        bool IsLazy { get; set; }
    }

    public interface ITypeBinding<T> : ITypeBinding
    {
    }
}
