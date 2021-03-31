using System;
using System.Collections.Generic;

namespace ManualDI.TypeFactories
{
    public class ConstraintAllContainerTypeFactory<T> : ITypeFactory<List<T>>
    {
        public Action<IResolutionConstraints> Constraints { get; }

        public ConstraintAllContainerTypeFactory(Action<IResolutionConstraints> constraints)
        {
            Constraints = constraints;
        }

        public List<T> Create(IDiContainer container)
        {
            return container.ResolveAll<T>(Constraints);
        }
    }
}
