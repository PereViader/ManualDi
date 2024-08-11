using System;

namespace ManualDi.Main
{
    public sealed class ResolutionConstraints
    {
        public Func<TypeBinding, bool>? ResolutionConstraintDelegate { get; set; }

        public bool Accepts(TypeBinding typeBinding)
        {
            if (ResolutionConstraintDelegate is null)
            {
                return true;
            }

            return ResolutionConstraintDelegate.Invoke(typeBinding);
        }
    }
}
