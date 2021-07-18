using ManualDi.TypeFactories;
using ManualDi.TypeInjections;
using ManualDi.TypeScopes;
using System.Collections.Generic;

namespace ManualDi
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
