using System.Collections.Generic;

namespace ManualDI.TypeFactories
{
    public class ContainerAllTypeFactory<T> : ITypeFactory<List<T>>
    {
        public List<T> Create(IDiContainer container)
        {
            return container.ResolveAll<T>();
        }
    }
}
