using System;
using System.Collections.Generic;

namespace ManualDi.Main
{
    public interface IDiContainerBindings
    {
        Dictionary<Type, List<ITypeBinding>> TypeBindings { get; }

        void AddBinding<T>(ITypeBinding<T> typeBinding);
    }
}
