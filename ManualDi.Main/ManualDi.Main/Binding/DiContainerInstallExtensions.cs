using System.Collections.Generic;

namespace ManualDi.Main
{
    public static class DiContainerInstallExtensions
    {
        public static DiContainerBindings Install(this DiContainerBindings diContainerBindings, IInstaller installer)
        {
            installer.Install(diContainerBindings);
            return diContainerBindings;
        }
        
        public static DiContainerBindings Install(this DiContainerBindings diContainerBindings, IEnumerable<IInstaller> installers)
        {
            foreach (var installer in installers)
            {
                installer.Install(diContainerBindings);
            }
            return diContainerBindings;
        }
        
        public static DiContainerBindings Install(this DiContainerBindings diContainerBindings, IEnumerable<InstallDelegate> installers)
        {
            foreach (var installer in installers)
            {
                installer.Invoke(diContainerBindings);
            }
            return diContainerBindings;
        }
        
        public static DiContainerBindings Install(this DiContainerBindings diContainerBindings, InstallDelegate installer)
        {
            installer.Invoke(diContainerBindings);
            return diContainerBindings;
        }
    }
}