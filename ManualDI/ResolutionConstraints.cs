using System;

namespace ManualDI
{
    public class ResolutionConstraints : IResolutionConstraints
    {
        public object Identifier { get; set; }
        public Func<ITypeMetadata, bool> TypeMetadata { get ; set; }

        public bool Accepts<T>(ITypeBinding<T> typeBinding)
        {
            if (Identifier != null && !object.Equals(typeBinding.Identifier,Identifier))
            {
                return false;
            }

            if(TypeMetadata != null && !TypeMetadata.Invoke(typeBinding.TypeMetadata))
            {
                return false;
            }

            return true;
        }
    }
}