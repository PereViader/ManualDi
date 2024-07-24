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

        public GenerationClassContext(StringBuilder stringBuilder, string className, INamedTypeSymbol classSymbol, string obsoleteText)
        {
            StringBuilder = stringBuilder;
            ClassName = className;
            ClassSymbol = classSymbol;
            ObsoleteText = obsoleteText;
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
        
        private void Generate(SourceProductionContext context, ((Compilation, ImmutableArray<ModuleInfo>), ImmutableArray<ClassDeclarationSyntax>) arg)
        {
            var ((compilation, moduleInfos), classDeclarations) = arg;

            //We don't generate if the ManualDi reference is not there
            if (moduleInfos.Length == 0)
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

                stringBuilder.Append(@"
using ManualDi.Main;

namespace ManualDi.Main
{
    public static partial class ManualDiGeneratedExtensions
    {
");
                
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = ModelExtensions.GetDeclaredSymbol(model, classDeclaration) as INamedTypeSymbol;
                
                if (classSymbol is null)
                {
                    continue;
                }
                
                var accessibility = GetSymbolAccessibility(classSymbol);
                if (accessibility is not (Accessibility.Public or Accessibility.Internal))
                {
                    continue;
                }

                bool inheritsUnityObject = InheritsFromSymbol(classSymbol, unityEngineObjectSymbol);
                var className = FullyQualifyType(classSymbol);
                var obsoleteText = IsSymbolObsolete(classSymbol)
                    ? "[System.Obsolete]\r\n        " 
                    : "";

                var generationContext = new GenerationClassContext(stringBuilder, className, classSymbol, obsoleteText);

                Accessibility? constructor = null;
                if (!inheritsUnityObject)
                {
                    constructor = AddFromConstructor(generationContext);
                }
                var initialize = AddInitialize(generationContext);
                var inject = AddInject(generationContext);
                
                if (constructor.HasValue)
                {
                    AddDefaultConstructor(generationContext, constructor, initialize, inject, accessibility);
                }
                AddDefaultInstance(generationContext, initialize, inject, accessibility);
                AddDefaultMethod(generationContext, initialize, inject, accessibility);

                stringBuilder.Append(@"
    }
}
");
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

        private static ClassDeclarationSyntax GetClassDeclaration(GeneratorSyntaxContext context, CancellationToken ct)
        {
            return (ClassDeclarationSyntax)context.Node;
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
            var arguments = CreateArgumentsResolution(constructor);

            context.StringBuilder.Append($@"
        {context.ObsoleteText}{accessibilityString} static TypeBinding<T, {context.ClassName}> FromConstructor<T>(this TypeBinding<T, {context.ClassName}> typeBinding)
        {{
            typeBinding.FromMethod(static c => new {context.ClassName}({arguments}));
            return typeBinding;
        }}
");
            return accessibility;
        }

        private static string CreateArgumentsResolution(IMethodSymbol constructor)
        {
            return string.Join(",\r\n                ", constructor.Parameters.Select(p => $"c.{CreateArgumentResolutionMethod(p.Type)}<{FullyQualifyType(p.Type)}>()"));
        }

        private static string CreateArgumentResolutionMethod(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.NullableAnnotation is not NullableAnnotation.Annotated)
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
            var arguments = CreateArgumentsResolution(initializeMethod);

            context.StringBuilder.Append($@"
        {context.ObsoleteText}{accessibilityString} static TypeBinding<T, {context.ClassName}> Initialize<T>(this TypeBinding<T, {context.ClassName}> typeBinding)
        {{
            typeBinding.Initialize(static (o, c) => o.Initialize({arguments}));
            return typeBinding;
        }}
");
            return accessibility;
        }

        private static Accessibility? AddInject(GenerationClassContext generationClassContext)
        {
            var injectMethod = generationClassContext.ClassSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .SingleOrDefault(m => m is { Name: "Inject", DeclaredAccessibility: Accessibility.Public or Accessibility.Internal, IsStatic: false });

            var injectProperties = generationClassContext.ClassSymbol
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
            
            generationClassContext.StringBuilder.AppendLine($@"
        {generationClassContext.ObsoleteText}{accessibilityString} static TypeBinding<T, {generationClassContext.ClassName}> Inject<T>(this TypeBinding<T, {generationClassContext.ClassName}> typeBinding)
        {{
            typeBinding.Inject(static (o, c) => 
            {{");

            foreach (var injectProperty in injectProperties)
            {
                generationClassContext.StringBuilder.AppendLine($"                o.{injectProperty.Name} = c.{CreateArgumentResolutionMethod(injectProperty.Type)}<{FullyQualifyType(injectProperty.Type)}>();");
            }
            
            if (injectMethod is not null)
            {
                var arguments = CreateArgumentsResolution(injectMethod);
                generationClassContext.StringBuilder.AppendLine($"                o.Inject({arguments});");
            }
            
            generationClassContext.StringBuilder.Append(@"            });
            return typeBinding;
        }
");
            return accessibility;
        }

        private static void AddDefaultConstructor(GenerationClassContext generationClassContext, Accessibility? constructor, Accessibility? initialize,
            Accessibility? inject, Accessibility typeAccessibility)
        {
            var accessibility = typeAccessibility;
            if (constructor.HasValue && constructor.Value < accessibility)
            {
                accessibility = constructor.Value;
            }
            
            if (initialize.HasValue && initialize.Value < accessibility)
            {
                accessibility = initialize.Value;
            }
            
            if (inject.HasValue && inject.Value < accessibility)
            {
                accessibility = inject.Value;
            }

            var accessibiliyString = GetAccessibilityString(accessibility);
            
            generationClassContext.StringBuilder.AppendLine($@"
        {generationClassContext.ObsoleteText}{accessibiliyString} static TypeBinding<T, {generationClassContext.ClassName}> Default<T>(this TypeBinding<T, {generationClassContext.ClassName}> typeBinding)
        {{");

            if (constructor.HasValue)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.FromConstructor();");
            }

            if (initialize.HasValue)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Initialize();");
            }

            if (inject.HasValue)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Inject();");
            }

            generationClassContext.StringBuilder.AppendLine("            return typeBinding;\r\n        }");
        }
        
        private static void AddDefaultInstance(GenerationClassContext generationClassContext, Accessibility? initialize,
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
            
            generationClassContext.StringBuilder.AppendLine($@"
        {generationClassContext.ObsoleteText}{accessibiliyString} static TypeBinding<T, {generationClassContext.ClassName}> Default<T>(this TypeBinding<T, {generationClassContext.ClassName}> typeBinding, {generationClassContext.ClassName} instance)
        {{");
            
            generationClassContext.StringBuilder.AppendLine("            typeBinding.FromInstance(instance);");

            if (initialize.HasValue)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Initialize();");
            }

            if (inject.HasValue)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Inject();");
            }

            generationClassContext.StringBuilder.AppendLine("            return typeBinding;\r\n        }");
        }
        
        private static void AddDefaultMethod(GenerationClassContext generationClassContext, Accessibility? initialize,
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

            generationClassContext.StringBuilder.AppendLine($@"
        {generationClassContext.ObsoleteText}{accessibiliyString} static TypeBinding<T, {generationClassContext.ClassName}> Default<T>(this TypeBinding<T, {generationClassContext.ClassName}> typeBinding, CreateDelegate<{generationClassContext.ClassName}> func)
        {{");
            
            generationClassContext.StringBuilder.AppendLine("            typeBinding.FromMethod(func);");

            if (inject.HasValue)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Inject();");
            }

            if (initialize.HasValue)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Initialize();");
            }

            generationClassContext.StringBuilder.AppendLine("            return typeBinding;\r\n        }");
        }

        private static readonly SymbolDisplayFormat FullyQualifyTypeSymbolDisplayFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
        );
        
        private static string FullyQualifyType(ITypeSymbol typeSymbol)
        {
            //I don't like that we cast this, but I could not find a way to do this without casting
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol &&
              namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                // Get the underlying non-nullable type (the first type argument of the nullable type)
                var underlyingType = namedTypeSymbol.TypeArguments[0];
                return underlyingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
            
            return typeSymbol.ToDisplayString(FullyQualifyTypeSymbolDisplayFormat);
        }
    }
}
