using System;

namespace ManualDI
{
    public interface IResolutionConstraints
    {
        object Identifier { get; set; }
        Func<ITypeMetadata, bool> TypeMetadata { get; set; }

        bool Accepts<T>(ITypeBinding<T> typeBinding);
    }
}