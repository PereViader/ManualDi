using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class ResolutionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints When(this ResolutionConstraints resolution, IsValidBindingDelegate isValidBindingDelegate)
        {
            var previous = resolution.IsValidBindingDelegate;
            resolution.IsValidBindingDelegate = previous is null 
                ? isValidBindingDelegate 
                : x => previous.Invoke(x) && isValidBindingDelegate.Invoke(x);
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints Id(this ResolutionConstraints resolution, object id)
        {
            var previous = resolution.IsValidBindingDelegate;
            resolution.IsValidBindingDelegate = previous is null 
                ? x => x.Id == id 
                : x => previous.Invoke(x) && x.Id == id;
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints InjectedInto<T>(this ResolutionConstraints resolution)
        {
            var previous = resolution.IsValidBindingDelegate;
            resolution.IsValidBindingDelegate = previous is null 
                ? x => x.InjectIntoType == typeof(T) 
                : x => previous.Invoke(x) && x.InjectIntoType == typeof(T);
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints InjectedInto(this ResolutionConstraints resolution, Type type)
        {
            var previous = resolution.IsValidBindingDelegate;
            resolution.IsValidBindingDelegate = previous is null 
                ? x => x.InjectIntoType == type 
                : x => previous.Invoke(x) && x.InjectIntoType == type;
            return resolution;
        }
    }
}
