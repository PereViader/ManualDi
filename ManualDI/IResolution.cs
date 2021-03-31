using System;

namespace ManualDI
{
    public interface IResolutionConstraints
    {
        Func<ITypeMetadata, bool> TypeMetadata { get; set; }

        bool Accepts<T>(ITypeBinding<T> typeBinding);
    }
}
