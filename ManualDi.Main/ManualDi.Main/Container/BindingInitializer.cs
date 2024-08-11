using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    internal sealed class BindingInitializer
    {
        private readonly List<List<Action<IDiContainer>>?> bindingInitializationCommands = new();
        private int nestedCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Queue(TypeBinding typeBinding, object instance)
        {
            if (!typeBinding.NeedsInitialize)
            {
                if (nestedCount >= bindingInitializationCommands.Count)
                {
                    bindingInitializationCommands.Add(null);
                }

                return;
            }
            List<Action<IDiContainer>>? commands;
        
            if (nestedCount >= bindingInitializationCommands.Count)
            {
                commands = new List<Action<IDiContainer>>(1);
                bindingInitializationCommands.Add(commands);
            }
            else
            {
                commands = bindingInitializationCommands[nestedCount];
                if (commands is null)
                {
                    commands = new List<Action<IDiContainer>>(1);
                    bindingInitializationCommands[nestedCount] = commands;
                }
            }

            commands.Add(c => typeBinding.InitializeObject(instance, c));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitializeCurrentLevelQueued(IDiContainer container)
        {
            nestedCount++;

            var index = bindingInitializationCommands.Count - 1;
            var commands = bindingInitializationCommands[index];
            bindingInitializationCommands.RemoveAt(index);
            if (commands is not null)
            {
                foreach (var action in commands)
                {
                    action.Invoke(container);
                }
            }

            nestedCount--;
        }
    }
}
