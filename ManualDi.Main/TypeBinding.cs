using ManualDi.Main.TypeFactories;
using ManualDi.Main.TypeInjections;
using ManualDi.Main.TypeScopes;
using System.Collections.Generic;

namespace ManualDi.Main
{
    public class TypeBinding<T> : ITypeBinding<T>
    {
        public ITypeMetadata TypeMetadata { get; set; }
        public ITypeScope TypeScope { get; set; }
        public ITypeFactory Factory { get; set; }
        public List<ITypeInjection> TypeInjections { get; set; }
        public bool IsLazy { get; set; }
    }
}
