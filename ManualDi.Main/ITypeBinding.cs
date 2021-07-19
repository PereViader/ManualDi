using ManualDi.Main.TypeFactories;
using ManualDi.Main.TypeInjections;
using ManualDi.Main.TypeScopes;
using System.Collections.Generic;

namespace ManualDi.Main
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
