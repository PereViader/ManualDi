using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class ResolutionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints When(this ResolutionConstraints resolution, FilterBindingDelegate filterBindingDelegate)
        {
            var previous = resolution.FilterBindingDelegate;
            resolution.FilterBindingDelegate = previous is null 
                ? filterBindingDelegate 
                : x => previous.Invoke(x) && filterBindingDelegate.Invoke(x);
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints Id(this ResolutionConstraints resolution, object id)
        {
            var previous = resolution.FilterBindingDelegate;
            resolution.FilterBindingDelegate = previous is null 
                ? x => x.TypeBinding.Id == id 
                : x => previous.Invoke(x) && x.TypeBinding.Id == id;
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints InjectedIntoType<T>(this ResolutionConstraints resolution)
        {
            var previous = resolution.FilterBindingDelegate;
            resolution.FilterBindingDelegate = previous is null 
                ? x => x.InjectedIntoTypeBinding?.ConcreteType == typeof(T) 
                : x => previous.Invoke(x) && x.InjectedIntoTypeBinding?.ConcreteType == typeof(T);
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints InjectedIntoType(this ResolutionConstraints resolution, Type type)
        {
            var previous = resolution.FilterBindingDelegate;
            resolution.FilterBindingDelegate = previous is null 
                ? x => x.InjectedIntoTypeBinding?.ConcreteType == type 
                : x => previous.Invoke(x) && x.InjectedIntoTypeBinding?.ConcreteType == type;
            return resolution;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResolutionConstraints InjectedIntoId(this ResolutionConstraints resolution, object id)
        {
            var previous = resolution.FilterBindingDelegate;
            resolution.FilterBindingDelegate = previous is null 
                ? x => x.InjectedIntoTypeBinding?.Id == id 
                : x => previous.Invoke(x) && x.InjectedIntoTypeBinding?.Id == id;
            return resolution;
        }
    }
}
