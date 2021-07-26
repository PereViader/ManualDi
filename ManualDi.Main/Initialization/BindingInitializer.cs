using System;
using System.Collections.Generic;

namespace ManualDi.Main.Initialization
{
    public class BindingInitializer : IBindingInitializer
    {
        private readonly List<Action> bindingInitializationCommands = new List<Action>();

        public void Injest(ITypeBinding typeBinding, object instance)
        {
            var bindingInitialization = typeBinding.BindingInitialization;
            if (bindingInitialization == null)
            {
                return;
            }

            bindingInitializationCommands.Add(() => bindingInitialization.Initialize(instance));
        }

        public void InitializeAllQueued()
        {
            foreach (var bindingInitialization in bindingInitializationCommands)
            {
                bindingInitialization.Invoke();
            }
            bindingInitializationCommands.Clear();
        }
    }
}
