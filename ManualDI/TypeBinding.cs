using ManualDi.TypeFactories;
using ManualDi.TypeInjections;
using ManualDi.TypeScopes;
using System.Collections.Generic;

namespace ManualDi
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
