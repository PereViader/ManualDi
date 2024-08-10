using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ManualDi.Main.Generators
{
    public readonly struct ModuleInfo
    {
        public string Name { get; }
        public Version Version { get; }

        public ModuleInfo(string name, Version version)
        {
            Name = name;
            Version = version;
        }
    }

    public static class MetadataReferenceExtensions
    {
        public static IEnumerable<ModuleInfo> GetModules(this MetadataReference metadataReference)
        {
            // Project reference (ISymbol)
            if (metadataReference is CompilationReference compilationReference)
            {
                return compilationReference.Compilation.Assembly.Modules
                    .Select(m => new ModuleInfo(
                        m.Name,
                        compilationReference.Compilation.Assembly.Identity.Version));
            }

            // DLL
            if (metadataReference is PortableExecutableReference portable
                && portable.GetMetadata() is AssemblyMetadata assemblyMetadata)
            {
                return assemblyMetadata.GetModules()
                    .Select(m => new ModuleInfo(
                        m.Name,
                        m.GetMetadataReader().GetAssemblyDefinition().Version));
            }

            return Array.Empty<ModuleInfo>();
        }
    }
}