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
    public readonly struct GenerationClassContext
    {
        public StringBuilder StringBuilder { get; }
        public string ClassName { get; }
        public INamedTypeSymbol ClassSymbol { get; }
        public string ObsoleteText { get; }
        public INamedTypeSymbol LazyTypeSymbol { get; }
        public INamedTypeSymbol ListTypeSymbol { get; }

        public GenerationClassContext(StringBuilder stringBuilder, string className, INamedTypeSymbol classSymbol,
            string obsoleteText, INamedTypeSymbol lazyTypeSymbol, INamedTypeSymbol listTypeSymbol)
        {
            StringBuilder = stringBuilder;
            ClassName = className;
            ClassSymbol = classSymbol;
            ObsoleteText = obsoleteText;
            LazyTypeSymbol = lazyTypeSymbol;
            ListTypeSymbol = listTypeSymbol;
        }
    }
    
    [Generator]
    public class ManualDiSourceGenerator : IIncrementalGenerator
    {
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
            
            var lazyTypeSymbol = compilation.GetTypeByMetadataName("System.Lazy`1");
            if (lazyTypeSymbol is null)
            {
                return;
            }
            
            var listTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");
            if (listTypeSymbol is null)
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

                var generationContext = new GenerationClassContext(stringBuilder, className, classSymbol, obsoleteText, lazyTypeSymbol, listTypeSymbol);

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
                .Any(x => x.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute");
        }
        
        private static bool IsSymbolInjectValid(IPropertySymbol propertySymbol)
        {
            if (propertySymbol.IsStatic)
            {
                return false;
            }
            
            bool isPropertyAccessible = propertySymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;
            if (!isPropertyAccessible)
            {
                return false;
            }

            bool isSetterAccessible = propertySymbol.SetMethod?.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;
            if (!isSetterAccessible)
            {
                return false;
            }
            
            bool hasInjectAttribute = propertySymbol
                .GetAttributes()
                .Any(x => x.AttributeClass?.ToDisplayString() == "ManualDi.Main.InjectAttribute");
            if (!hasInjectAttribute)
            {
                return false;
            }
            
            return true;
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
            var arguments = CreateMethodResolution(constructor, context.LazyTypeSymbol, context.ListTypeSymbol);
            
            context.StringBuilder.AppendLine($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{context.ObsoleteText}}{{accessibilityString}} static TypeBinding<T, {{context.ClassName}}> FromConstructor<T>(this TypeBinding<T, {{context.ClassName}}> typeBinding)
                    {
                        return typeBinding.FromMethod(static c => new {{context.ClassName}}({{arguments}}));
                    }
                    
            """);
            return accessibility;
        }

        private static string CreateMethodResolution(IMethodSymbol constructor, INamedTypeSymbol lazyTypeSymbol, INamedTypeSymbol listTypeSymbol)
        {
            return string.Join(",\r\n                ", constructor.Parameters.Select(x => CreteTypeResolution(x.Type, lazyTypeSymbol, listTypeSymbol)));
        }

        private static string CreteTypeResolution(ITypeSymbol typeSymbol, INamedTypeSymbol lazyTypeSymbol, INamedTypeSymbol listTypeSymbol)
        {
            var lazyGenericType = TryGeneric1Type(typeSymbol, lazyTypeSymbol);
            if (lazyGenericType is not null)
            {
                return $"new System.Lazy<{FullyQualifyTypeWithNullable(lazyGenericType)}>(() => c.{CreateContainerResolutionMethod(lazyGenericType)}<{FullyQualifyTypeWithoutNullable(lazyGenericType)}>())";
            }
            
            var listGenericType = TryGeneric1Type(typeSymbol, listTypeSymbol);
            if (listGenericType is not null && !IsNullableTypeSymbol(listGenericType))
            {
                return $"c.ResolveAll<{FullyQualifyTypeWithoutNullable(listGenericType)}>()";
            }
            
            return $"c.{CreateContainerResolutionMethod(typeSymbol)}<{FullyQualifyTypeWithoutNullable(typeSymbol)}>()";
        }
        
        public static ITypeSymbol? TryGeneric1Type(ITypeSymbol typeSymbol, INamedTypeSymbol lazyTypeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, lazyTypeSymbol))
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
            var arguments = CreateMethodResolution(initializeMethod, context.LazyTypeSymbol, context.ListTypeSymbol);

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
                .Where(IsSymbolInjectValid)
                .ToArray();

            if (injectMethod is null && injectProperties.Length == 0)
            {
                return null;
            }

            var injectPropertiesAccessibility = injectProperties.Length == 0
                ? Accessibility.Public
                : injectProperties
                    .Select(GetSymbolAccessibility)
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

            foreach (var injectProperty in injectProperties)
            {
                context.StringBuilder.AppendLine($"                o.{injectProperty.Name} = {CreteTypeResolution(injectProperty.Type, context.LazyTypeSymbol, context.ListTypeSymbol)};");
            }
            
            if (injectMethod is not null)
            {
                var arguments = CreateMethodResolution(injectMethod, context.LazyTypeSymbol, context.ListTypeSymbol);
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

        private static readonly SymbolDisplayFormat FullyQualifyTypeSymbolDisplayFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
        );
        
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
