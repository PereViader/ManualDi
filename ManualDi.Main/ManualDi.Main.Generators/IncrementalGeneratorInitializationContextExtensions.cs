using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace ManualDi.Main.Generators
{
    public static class IncrementalGeneratorInitializationContextExtensions
    {
        public static IncrementalValuesProvider<MetadataReference> GetMetadataReferencesProvider(
            this IncrementalGeneratorInitializationContext context)
        {
            var metadataProviderProperty = context.GetType()
                .GetProperty(nameof(context.MetadataReferencesProvider))
                ?? throw new Exception($"The property '{nameof(context.MetadataReferencesProvider)}' not found");

            var metadataProvider = metadataProviderProperty.GetValue(context);

            return metadataProvider switch
            {
                IncrementalValuesProvider<MetadataReference> metadataValuesProvider => metadataValuesProvider,
                IncrementalValueProvider<MetadataReference> metadataValueProvider => metadataValueProvider
                    .SelectMany(static (reference, _) => ImmutableArray.Create(reference)),
                _ => throw new Exception($"The '{nameof(context.MetadataReferencesProvider)}' is neither an 'IncrementalValuesProvider<{nameof(MetadataReference)}>' nor an 'IncrementalValueProvider<{nameof(MetadataReference)}>.'")
            };
        }
    }
}