using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;


namespace ManualDi.Main.Generators
{
    public record struct GenerationClassContext(StringBuilder StringBuilder, string ClassName, INamedTypeSymbol ClassSymbol, string ObsoleteText);
    
    [Generator]
    public class ManualDiSourceGenerator : IIncrementalGenerator
    {
        private static INamedTypeSymbol LazyTypeSymbol = default!;
        private static INamedTypeSymbol ListTypeSymbol = default!;
        private static INamedTypeSymbol IListTypeSymbol = default!;
        private static INamedTypeSymbol IReadOnlyListTypeSymbol = default!;
        private static INamedTypeSymbol IEnumerableTypeSymbol = default!;
        private static INamedTypeSymbol IReadOnlyCollectionTypeSymbol = default!;
        private static INamedTypeSymbol ICollectionTypeSymbol = default!;
        private static INamedTypeSymbol InjectAttributeTypeSymbol = default!;
        private static INamedTypeSymbol ObsoleteAttributeTypeSymbol = default!;
        
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //https://www.thinktecture.com/en/net/roslyn-source-generators-code-according-to-dependencies/
            // Create a provider for metadata references
            var manualDiMainReferenceModule = context.GetMetadataReferencesProvider()
                .SelectMany((references, _) => references.GetModules())
                .Where(x => x.Name is "ManualDi.Main.dll")
                .Collect();
            
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(IsSyntaxNodeValid, GetClassDeclaration)
                .Collect();

            var providers = context.CompilationProvider
                .Combine(manualDiMainReferenceModule)
                .Combine(classDeclarations);

            context.RegisterSourceOutput(providers, Generate);
        }
        
        private static bool IsSyntaxNodeValid(SyntaxNode node, CancellationToken ct)
        {
            if (node is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                return false;
            }

            if (classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return false;
            }

            if (classDeclarationSyntax.TypeParameterList?.Parameters.Count > 0)
            {
                return false;
            }

            return true;
        }
        
        private static ClassDeclarationSyntax GetClassDeclaration(GeneratorSyntaxContext context, CancellationToken ct)
        {
            return (ClassDeclarationSyntax)context.Node;
        }
        
        private void Generate(SourceProductionContext context, ((Compilation, ImmutableArray<ModuleInfo>), ImmutableArray<ClassDeclarationSyntax>) arg)
        {
            var ((compilation, moduleInfos), classDeclarations) = arg;

            //We don't generate if the ManualDi reference is not there
            if (moduleInfos.Length == 0)
            {
                return;
            }

            if (!TryInitializeStaticTypeSymbols(compilation, context))
            {
                return;
            }

            var unityEngineObjectSymbol = compilation.GetTypeByMetadataName("UnityEngine.Object");

            foreach (var classDeclaration in classDeclarations)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                var stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("""
                using System.Runtime.CompilerServices;
                
                namespace ManualDi.Main
                {
                    public static partial class ManualDiGeneratedExtensions
                    {
                """);
                
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                if (ModelExtensions.GetDeclaredSymbol(model, classDeclaration) is not INamedTypeSymbol classSymbol)
                {
                    continue;
                }
                
                var accessibility = GetSymbolAccessibility(classSymbol);
                if (accessibility is not (Accessibility.Public or Accessibility.Internal))
                {
                    continue;
                }

                bool inheritsUnityObject = InheritsFromSymbol(classSymbol, unityEngineObjectSymbol);
                var className = FullyQualifyTypeWithoutNullable(classSymbol);
                var obsoleteText = IsSymbolObsolete(classSymbol)
                    ? "[System.Obsolete]\r\n        " 
                    : "";

                var generationContext = new GenerationClassContext(stringBuilder, className, classSymbol, obsoleteText);

                if (!inheritsUnityObject)
                {
                    AddFromConstructor(generationContext);
                }
                var initialize = AddInitialize(generationContext);
                var inject = AddInject(generationContext);
                
                AddDefault(generationContext, initialize, inject, accessibility);

                stringBuilder.AppendLine("""
                    }
                }
                """);
                
                context.AddSource($"ManualDiGeneratedExtensions.{className}.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
            }
        }

        private static bool TryInitializeStaticTypeSymbols(Compilation compilation, SourceProductionContext context)
        {
            var lazyTypeSymbol = compilation.GetTypeByMetadataName("System.Lazy`1");
            if (lazyTypeSymbol is null)
            {
                ReportTypeNotFound("System.Lazy<T>", context);
                return false;
            }
            LazyTypeSymbol = lazyTypeSymbol;

            var listTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");
            if (listTypeSymbol is null)
            {
                ReportTypeNotFound("System.Collections.Generic.List<T>", context);
                return false;
            }
            ListTypeSymbol = listTypeSymbol;

            var ilistTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1");
            if (ilistTypeSymbol is null)
            {
                ReportTypeNotFound("System.Collections.Generic.IList<T>", context);
                return false;
            }
            IListTypeSymbol = ilistTypeSymbol;

            var ireadOnlyListTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1");
            if (ireadOnlyListTypeSymbol is null)
            {
                ReportTypeNotFound("System.Collections.Generic.IReadOnlyList<T>", context);
                return false;
            }
            IReadOnlyListTypeSymbol = ireadOnlyListTypeSymbol;

            var ireadOnlyCollectionTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");
            if (ireadOnlyCollectionTypeSymbol is null)
            {
                ReportTypeNotFound("System.Collections.Generic.IReadOnlyCollection<T>", context);
                return false;
            }
            IReadOnlyCollectionTypeSymbol = ireadOnlyCollectionTypeSymbol;

            var iCollectionTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1");
            if (iCollectionTypeSymbol is null)
            {
                ReportTypeNotFound("System.Collections.Generic.ICollection<T>", context);
                return false;
            }
            ICollectionTypeSymbol = iCollectionTypeSymbol;

            var ienumerableTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
            if (ienumerableTypeSymbol is null)
            {
                ReportTypeNotFound("System.Collections.Generic.IEnumerable<T>", context);
                return false;
            }
            IEnumerableTypeSymbol = ienumerableTypeSymbol;

            var injectAttributeTypeSymbol = compilation.GetTypeByMetadataName("ManualDi.Main.InjectAttribute");
            if (injectAttributeTypeSymbol is null)
            {
                ReportTypeNotFound("ManualDi.Main.InjectAttribute", context);
                return false;
            }
            InjectAttributeTypeSymbol = injectAttributeTypeSymbol;

            var obsoleteAttributeTypeSymbol = compilation.GetTypeByMetadataName("System.ObsoleteAttribute");
            if (obsoleteAttributeTypeSymbol is null)
            {
                ReportTypeNotFound("System.ObsoleteAttribute", context);
                return false;
            }
            ObsoleteAttributeTypeSymbol = obsoleteAttributeTypeSymbol;

            return true;
        }

        private static void ReportTypeNotFound(string typeName, SourceProductionContext context)
        {
            var descriptor = new DiagnosticDescriptor(
                id: "MDI001",
                title: "Type Not Found",
                messageFormat: "The type '{0}' could not be found. Ensure the necessary assemblies are referenced.",
                category: "ManualDiGenerator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            var diagnostic = Diagnostic.Create(descriptor, Location.None, typeName);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool InheritsFromSymbol(INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol? inheritedNameTypeSymbol)
        {
            if (inheritedNameTypeSymbol == null)
            {
                return false;
            }

            var baseType = namedTypeSymbol;
            while (baseType != null)
            {
                if (SymbolEqualityComparer.Default.Equals(baseType, inheritedNameTypeSymbol))
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }
        
        private static bool IsSymbolObsolete(ISymbol typeSymbol)
        {
            return typeSymbol.GetAttributes()
                .Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, ObsoleteAttributeTypeSymbol));
        }

        private static AttributeData? GetInjectAttribute(IPropertySymbol propertySymbol)
        {
            if (propertySymbol.IsStatic)
            {
                return null;
            }
            
            bool isSetterAccessible = propertySymbol.SetMethod?.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;
            if (!isSetterAccessible)
            {
                return null;
            }
            
            bool isPropertyAccessible = propertySymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;
            if (!isPropertyAccessible)
            {
                return null;
            }

            return propertySymbol
                .GetAttributes()
                .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, InjectAttributeTypeSymbol));
        }
        
        private static AttributeData? GetInjectAttribute(ISymbol parameterSymbol)
        {
            return parameterSymbol
                .GetAttributes()
                .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, InjectAttributeTypeSymbol));
        }

        private static string? GetInjectId(AttributeData attributeData)
        {
            if (attributeData.ConstructorArguments.Length != 1)
            {
                return null;
            }

            var idArgument = attributeData.ConstructorArguments[0];
            if (idArgument.Value is null)
            {
                return null;
            }
                
            return $"\"{idArgument.Value}\"";
        }

        private static string GetAccessibilityString(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Internal => "internal",
                Accessibility.Protected => "protected",
                Accessibility.Private => "private",
                Accessibility.Public => "public",
                _ => throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, null),
            };
        }
        
        private static Accessibility GetSymbolAccessibility(ISymbol symbol)
        {
            // Recursively determine the effective accessibility of the type
            var visibility = symbol.DeclaredAccessibility;
            var currentSymbol = symbol.ContainingType;

            while (currentSymbol is not null)
            {
                var parentVisibility = currentSymbol.DeclaredAccessibility;
                if (parentVisibility < symbol.DeclaredAccessibility)
                {
                    visibility = parentVisibility;
                }
                currentSymbol = currentSymbol.ContainingType;
            }

            return visibility;
        }

        private static Accessibility? AddFromConstructor(GenerationClassContext context)
        {
            var constructor = context.ClassSymbol
                .Constructors
                .SingleOrDefault(c => c.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal);

            if (constructor is null)
            {
                return null;
            }

            var accessibility = GetSymbolAccessibility(constructor);
            var accessibilityString = GetAccessibilityString(accessibility);
            var arguments = CreateMethodResolution(constructor);
            
            context.StringBuilder.AppendLine($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{context.ObsoleteText}}{{accessibilityString}} static TypeBinding<T, {{context.ClassName}}> FromConstructor<T>(this TypeBinding<T, {{context.ClassName}}> typeBinding)
                    {
                        return typeBinding.FromMethod(static c => new {{context.ClassName}}({{arguments}}));
                    }
                    
            """);
            return accessibility;
        }

        private static string CreateMethodResolution(IMethodSymbol constructor)
        {
            return string.Join(",\r\n                ", constructor.Parameters.Select(x =>
            {
                var attribute = GetInjectAttribute(x);
                var id = attribute is null ? null : GetInjectId(attribute);
                return CreteTypeResolution(x.Type, id);
            }));
        }

        private static string CreteTypeResolution(ITypeSymbol typeSymbol, string? id)
        {
            var lazyGenericType = TryGenericLazyType(typeSymbol);
            if (lazyGenericType is not null)
            {
                return $"new System.Lazy<{FullyQualifyTypeWithNullable(lazyGenericType)}>(() => {CreteTypeResolution(lazyGenericType, id)})";
            }

            var listGenericType = TryGetEnumerableType(typeSymbol);
            if (listGenericType is not null && !IsNullableTypeSymbol(listGenericType))
            {
                return id is null 
                    ? $"c.ResolveAll<{FullyQualifyTypeWithoutNullable(listGenericType)}>()" 
                    : $"c.ResolveAll<{FullyQualifyTypeWithoutNullable(listGenericType)}>(x => x.WhereId({id}))";
            }

            return id is null 
                ? $"c.{CreateContainerResolutionMethod(typeSymbol)}<{FullyQualifyTypeWithoutNullable(typeSymbol)}>()" 
                : $"c.{CreateContainerResolutionMethod(typeSymbol)}<{FullyQualifyTypeWithoutNullable(typeSymbol)}>(x => x.WhereId({id}))";
        }

        private static ITypeSymbol? GetSpecialTypeList(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.SpecialType is SpecialType.System_Collections_Generic_IReadOnlyList_T
                or SpecialType.System_Collections_Generic_IList_T
                or SpecialType.System_Collections_Generic_IEnumerable_T)
            {
                if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
                {
                    return namedTypeSymbol.TypeArguments[0];
                }
            }

            return null;
        }
        
        public static ITypeSymbol? TryGenericLazyType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, LazyTypeSymbol))
                {
                    return namedTypeSymbol.TypeArguments[0];
                }
            }

            return null;
        }

        public static ITypeSymbol? TryGetEnumerableType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, ListTypeSymbol) ||
                    SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IReadOnlyListTypeSymbol) ||
                    SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IListTypeSymbol) || 
                    SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, ICollectionTypeSymbol) || 
                    SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IReadOnlyCollectionTypeSymbol) || 
                    SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IEnumerableTypeSymbol))
                {
                    return namedTypeSymbol.TypeArguments[0];
                }
            }

            return null;
        }

        private static string CreateContainerResolutionMethod(ITypeSymbol typeSymbol)
        {
            if (!IsNullableTypeSymbol(typeSymbol))
            {
                return "Resolve";
            }

            if (typeSymbol.IsValueType)
            {
                return "ResolveNullableValue";
            }

            return "ResolveNullable";
        }

        private static Accessibility? AddInitialize(GenerationClassContext context)
        {
            var initializeMethod = context.ClassSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .SingleOrDefault(m => m is { Name: "Initialize", DeclaredAccessibility: Accessibility.Public or Accessibility.Internal, IsStatic: false });

            if (initializeMethod is null)
            {
                return null;
            }

            var accessibility = GetSymbolAccessibility(initializeMethod);
            var accessibilityString = GetAccessibilityString(accessibility);
            var arguments = CreateMethodResolution(initializeMethod);

            context.StringBuilder.AppendLine($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{context.ObsoleteText}}{{accessibilityString}} static TypeBinding<T, {{context.ClassName}}> Initialize<T>(this TypeBinding<T, {{context.ClassName}}> typeBinding)
                    {
                        return typeBinding.Initialize(static (o, c) => o.Initialize({{arguments}}));
                    }
                    
            """);
            return accessibility;
        }

        private static Accessibility? AddInject(GenerationClassContext context)
        {
            var injectMethod = context.ClassSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .SingleOrDefault(m => m is { Name: "Inject", DeclaredAccessibility: Accessibility.Public or Accessibility.Internal, IsStatic: false });

            var injectProperties = context.ClassSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(x => (propertySymbol: x, attribute: GetInjectAttribute(x)))
                .Where(x => x.attribute is not null)
                .ToArray();

            if (injectMethod is null && injectProperties.Length == 0)
            {
                return null;
            }

            var injectPropertiesAccessibility = injectProperties.Length == 0
                ? Accessibility.Public
                : injectProperties
                    .Select(x => GetSymbolAccessibility(x.propertySymbol))
                    .Min();
            
            var methodAccessibility = injectMethod is null
                ? Accessibility.Public
                : GetSymbolAccessibility(injectMethod);

            var accessibility = (Accessibility)Math.Min((int)injectPropertiesAccessibility, (int)methodAccessibility);
            
            var accessibilityString = GetAccessibilityString(accessibility);
            
            context.StringBuilder.AppendLine($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{context.ObsoleteText}}{{accessibilityString}} static TypeBinding<T, {{context.ClassName}}> Inject<T>(this TypeBinding<T, {{context.ClassName}}> typeBinding)
                    {
                        return typeBinding.Inject(static (o, c) => 
                        {
            """);

            foreach (var (injectProperty, attribute) in injectProperties)
            {
                var id = attribute is null ? null : GetInjectId(attribute);
                context.StringBuilder.AppendLine($"                o.{injectProperty.Name} = {CreteTypeResolution(injectProperty.Type, id)};");
            }
            
            if (injectMethod is not null)
            {
                var arguments = CreateMethodResolution(injectMethod);
                context.StringBuilder.AppendLine($"                o.Inject({arguments});");
            }
                        
            context.StringBuilder.AppendLine("""
                        });
                    }
                    
            """);
            return accessibility;
        }

        private static void AddDefault(GenerationClassContext generationClassContext, Accessibility? initialize,
            Accessibility? inject, Accessibility typeAccessibility)
        {
            var accessibility = typeAccessibility;
            if (initialize.HasValue && initialize.Value < accessibility)
            {
                accessibility = initialize.Value;
            }
            
            if (inject.HasValue && inject.Value < accessibility)
            {
                accessibility = inject.Value;
            }

            var accessibiliyString = GetAccessibilityString(accessibility);
            
            generationClassContext.StringBuilder.Append($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{generationClassContext.ObsoleteText}}{{accessibiliyString}} static TypeBinding<T, {{generationClassContext.ClassName}}> Default<T>(this TypeBinding<T, {{generationClassContext.ClassName}}> typeBinding)
                    {
                        return typeBinding
            """);

            if (initialize.HasValue)
            {
                generationClassContext.StringBuilder.Append(".Initialize()");
            }

            if (inject.HasValue)
            {
                generationClassContext.StringBuilder.Append(".Inject()");
            }

            generationClassContext.StringBuilder.AppendLine("""
            ;
                    }
            """);
        }
        
        private static string FullyQualifyTypeWithoutNullable(ITypeSymbol typeSymbol)
        {
            var nonNullableType = GetNonNullableType(typeSymbol);
            if (nonNullableType is not null)
            {
                return nonNullableType.ToDisplayString();
            }
            
            return typeSymbol.ToDisplayString();
        }
        
        private static bool IsNullableTypeSymbol(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated || typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                return true;
            }
            
            return false;
        }
        
        public static ITypeSymbol? GetNonNullableType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && 
                typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                return namedTypeSymbol.TypeArguments[0];
            }
            
            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            }

            return null;
        }

        private static string FullyQualifyTypeWithNullable(ITypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString();
        }
    }
}
