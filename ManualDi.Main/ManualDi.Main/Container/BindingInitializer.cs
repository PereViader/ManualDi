using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    internal sealed class BindingInitializer
    {
        private readonly List<Action<IDiContainer>> bindingInitializationCommands = new();
        private readonly List<ushort> initializationsOnEachLevel = new();
        private int nestedCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Queue(TypeBinding typeBinding, object instance)
        {
            if (!typeBinding.NeedsInitialize)
            {
                if (nestedCount >= bindingInitializationCommands.Count)
                {
                    initializationsOnEachLevel.Add(0);
                }
                return;
            }
        
            if (nestedCount >= initializationsOnEachLevel.Count)
            {
                initializationsOnEachLevel.Add(1);
            }
            else
            {
                initializationsOnEachLevel[nestedCount]++;
            }

            bindingInitializationCommands.Add(c => typeBinding.InitializeObject(instance, c));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitializeCurrentLevelQueued(IDiContainer container)
        {
            nestedCount++;

            var removeIndex = initializationsOnEachLevel.Count - 1;
            var toDelete = initializationsOnEachLevel[removeIndex];
            
            bindingInitializationCommands.Reverse(bindingInitializationCommands.Count - toDelete, toDelete);
            
            while (toDelete > 0)
            {
                toDelete--;
                
                var lastIndex = bindingInitializationCommands.Count - 1;
                var element = bindingInitializationCommands[lastIndex];
                bindingInitializationCommands.RemoveAt(lastIndex);
                
                element.Invoke(container);
            }
            
            initializationsOnEachLevel.RemoveAt(removeIndex);

            nestedCount--;
        }
    }
}
