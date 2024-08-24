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
        public static ResolutionConstraints WhereMetadata(this ResolutionConstraints resolution, object flag)
        {
            var previous = resolution.ResolutionConstraintDelegate;
            if (previous is null)
            {
                resolution.ResolutionConstraintDelegate = x => x.Metadata?.ContainsKey(flag) == true;
            }
            else
            {
                resolution.ResolutionConstraintDelegate = x => previous.Invoke(x) && x.Metadata?.ContainsKey(flag) == true;
            }
            return resolution;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints WhereMetadata<T>(this ResolutionConstraints resolution, object key, T value)
        {
            var previous = resolution.ResolutionConstraintDelegate;
            if (previous is null)
            {
                resolution.ResolutionConstraintDelegate = x => Check(key, value, x);
            }
            else
            {
                resolution.ResolutionConstraintDelegate = x => previous.Invoke(x) && Check(key, value, x);
            }
            return resolution;

            static bool Check<Y>(object key, Y value, TypeBinding x)
            {
                if (x.Metadata is null)
                {
                    return false;
                }

                return x.Metadata.TryGetValue(key, out var objValue) && value!.Equals(objValue!);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints WhereMetadata<T>(this ResolutionConstraints resolution, object key, Func<T, bool> predicate)
        {
            var previous = resolution.ResolutionConstraintDelegate;
            if (previous is null)
            {
                resolution.ResolutionConstraintDelegate = x => Check(key, predicate, x);
            }
            else
            {
                resolution.ResolutionConstraintDelegate = x => previous.Invoke(x) && Check(key, predicate, x);
            }
            return resolution;

            static bool Check(object key, Func<T, bool> predicate, TypeBinding x)
            {
                if (x.Metadata is null)
                {
                    return false;
                }
                
                return x.Metadata.TryGetValue(key, out var objValue) && objValue is T value && predicate.Invoke(value);
            }
        }
    }
}
