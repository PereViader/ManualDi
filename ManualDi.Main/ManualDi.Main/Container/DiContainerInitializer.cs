using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    //In order to optimize the container, this is a struct that is modified by static ref extensions
    internal struct DiContainerInitializer
    {
        // This method has been optimized by storing the initialization commands that may happen at different depth levels
        // in the same list, thus having them be closer in memory and thus more cache friendly
        // however this requires us to track how many of those commands happen on each depth level
        // so we can keep track of how many belong to each depth level
        public readonly List<(TypeBinding typeBinding, object instance)> Initializations;
        public readonly List<ushort> InitializationsOnDepth;
        public int NestedCount;

        /// <summary>
        /// If you want to optimize your program for your use case and gain a little bit of time because the application won't need to dynamically change the list sizes
        /// provide the proper counts for each of the lists 
        /// </summary>
        public DiContainerInitializer(int? initializationsCount = null, int? initializationsOnDepthCount = null)
        {
            Initializations = initializationsCount.HasValue ? new(initializationsCount.Value) : new();
            InitializationsOnDepth = initializationsOnDepthCount.HasValue ? new(initializationsOnDepthCount.Value) : new();
            InitializationsOnDepth.Add(0);
            NestedCount = 0;
        }
    }
    
    internal static class DiContainerInitializerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueInitialize(ref this DiContainerInitializer o, TypeBinding typeBinding, object instance)
        {
            o.InitializationsOnDepth[o.NestedCount]++;
            o.Initializations.Add((typeBinding, instance));
        }

        public static void InitializeCurrentLevelQueued(ref this DiContainerInitializer o, IDiContainer container)
        {
            o.NestedCount++;
            o.InitializationsOnDepth.Add(0);

            var levelIndex = o.NestedCount - 1;
            var initializationCount = o.InitializationsOnDepth[levelIndex];
            var initializationStartIndex = o.Initializations.Count - initializationCount;
            
            for (int i = initializationStartIndex; i < o.Initializations.Count; i++)
            {
                var (typeBinding, instance) = o.Initializations[i];
                typeBinding.InitializeObject(instance, container);
            }
            
            o.Initializations.RemoveRange(initializationStartIndex, initializationCount);

            if (levelIndex > 0)
            {
                o.InitializationsOnDepth.RemoveAt(levelIndex);
            }
            else
            {
                o.InitializationsOnDepth[levelIndex] = 0;
            }

            o.NestedCount--;
        }   
    }
}
