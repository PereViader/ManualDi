using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public sealed class ResolutionConstraints
    {
        public Func<TypeBinding, bool>? ResolutionConstraintDelegate { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Accepts(TypeBinding typeBinding)
        {
            return ResolutionConstraintDelegate?.Invoke(typeBinding) == true;
        }
    }
}
