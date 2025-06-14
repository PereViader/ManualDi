using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    //In order to optimize the container, this is a struct that is modified by static ref extensions
    internal struct DiContainerInitializer
    {
        // This method has been optimized by storing the initialization commands that may happen at different depth levels
        // in the same list, thus having them be closer in memory and thus more cache friendly
        // however this requires us to track how many of those commands happen on each depth level
        // so we can keep track of how many belong to each depth level
        public readonly List<(InstanceContainerDelegate initializeBinding, object instance)> Initializations;
        public int CurrentDepthInitializations;

        /// <summary>
        /// If you want to optimize your program for your use case and gain a little bit of time because the application won't need to dynamically change the list sizes
        /// provide the proper counts for each of the lists 
        /// </summary>
        public DiContainerInitializer(int? initializationsCount = null)
        {
            Initializations = initializationsCount.HasValue ? new(initializationsCount.Value) : new();
            CurrentDepthInitializations = 0;
        }
    }
    
    internal static class DiContainerInitializerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueInitialize(ref this DiContainerInitializer o, InstanceContainerDelegate initializeDelegate, object instance)
        {
            o.CurrentDepthInitializations += 1;
            o.Initializations.Add((initializeDelegate, instance));
        }

        public static void InitializeCurrentLevelQueued(ref this DiContainerInitializer o, DiContainer container)
        {
            var initializationCount = o.CurrentDepthInitializations;
            var initializationStartIndex = o.Initializations.Count - o.CurrentDepthInitializations;
            o.CurrentDepthInitializations = 0;
            
            for (int i = initializationStartIndex; i < o.Initializations.Count; i++)
            {
                var (initializeDelegate, instance) = o.Initializations[i];
                initializeDelegate.Invoke(instance, container);
            }
            
            o.Initializations.RemoveRange(initializationStartIndex, initializationCount);
        }   
    }
}
