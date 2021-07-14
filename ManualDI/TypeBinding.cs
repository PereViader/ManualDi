using ManualDI.TypeFactories;
using ManualDI.TypeInjections;
using ManualDI.TypeScopes;
using System.Collections.Generic;

namespace ManualDI
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
