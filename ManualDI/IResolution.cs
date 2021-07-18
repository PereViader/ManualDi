using System;

namespace ManualDi
{
    public interface IResolutionConstraints
    {
        Func<ITypeMetadata, bool> TypeMetadata { get; set; }

        bool Accepts<T>(ITypeBinding<T> typeBinding);
    }
}
