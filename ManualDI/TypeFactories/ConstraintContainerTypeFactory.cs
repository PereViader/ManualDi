using System;

namespace ManualDI.TypeFactories
{
    public class ConstraintContainerTypeFactory<T> : ITypeFactory<T>
    {
        public Action<IResolutionConstraints> Constraints { get; }

        public ConstraintContainerTypeFactory(Action<IResolutionConstraints> constraints)
        {
            Constraints = constraints;
        }

        public T Create(IDiContainer container)
        {
            return container.Resolve<T>(Constraints);
        }
    }
}
