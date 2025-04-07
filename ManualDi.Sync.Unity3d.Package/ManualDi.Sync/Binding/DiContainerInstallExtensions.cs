using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class DiContainerInstallExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DiContainerBindings Install(this DiContainerBindings diContainerBindings, IInstaller installer)
        {
            installer.Install(diContainerBindings);
            return diContainerBindings;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DiContainerBindings Install(this DiContainerBindings diContainerBindings, IEnumerable<IInstaller> installers)
        {
            foreach (var installer in installers)
            {
                installer.Install(diContainerBindings);
            }
            return diContainerBindings;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DiContainerBindings Install(this DiContainerBindings diContainerBindings, IEnumerable<InstallDelegate> installers)
        {
            foreach (var installer in installers)
            {
                installer.Invoke(diContainerBindings);
            }
            return diContainerBindings;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DiContainerBindings Install(this DiContainerBindings diContainerBindings, InstallDelegate installer)
        {
            installer.Invoke(diContainerBindings);
            return diContainerBindings;
        }
    }
}