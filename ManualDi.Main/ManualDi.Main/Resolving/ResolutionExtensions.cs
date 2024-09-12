using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class ResolutionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints Where(this ResolutionConstraints resolution, Func<TypeBinding, bool> typeBindingFunc)
        {
            var previous = resolution.ResolutionConstraintDelegate;
            if (previous is null)
            {
                resolution.ResolutionConstraintDelegate = typeBindingFunc;
            }
            else
            {
                resolution.ResolutionConstraintDelegate = x => previous.Invoke(x) && typeBindingFunc.Invoke(x);
            }

            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints WhereId(this ResolutionConstraints resolution, object id)
        {
            var previous = resolution.ResolutionConstraintDelegate;
            if (previous is null)
            {
                resolution.ResolutionConstraintDelegate = x => x.Id == id;
            }
            else
            {
                resolution.ResolutionConstraintDelegate = x => previous.Invoke(x) && x.Id == id;
            }
            return resolution;
        }
    }
}
