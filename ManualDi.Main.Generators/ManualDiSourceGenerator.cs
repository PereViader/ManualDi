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
        
        private Accessibility GetTypeVisibility(INamedTypeSymbol typeSymbol)
        {
            // Recursively determine the effective accessibility of the type
            var visibility = typeSymbol.DeclaredAccessibility;
            var containingType = typeSymbol.ContainingType;

            while (containingType != null)
            {
                var parentVisibility = containingType.DeclaredAccessibility;
                if (parentVisibility < typeSymbol.DeclaredAccessibility)
                {
                    visibility = parentVisibility;
                }
                containingType = containingType.ContainingType;
            }

            return visibility;
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

            foreach (var classDeclaration in classDeclarations)
            {
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
                
                var accessibility = GetTypeVisibility(classSymbol);
                if (accessibility is not (Accessibility.Public or Accessibility.Internal))
                {
                    continue;
                }

                var accessibilityString = GetAccessibilityString(accessibility);
                var className = FullyQualifyType(classSymbol);
                var obsoleteText = IsSymbolObsolete(classSymbol)
                    ? "[System.Obsolete]\r\n        " 
                    : "";
                
                bool hasConstructor = AddFromConstructor(stringBuilder, className, classSymbol, accessibilityString, obsoleteText);
                bool hasInitialize = AddInitialize(stringBuilder, className, classSymbol, accessibilityString, obsoleteText);
                bool hasInject = AddInject(stringBuilder, className, classSymbol, accessibilityString, obsoleteText);
                AddDefaultConstructor(stringBuilder, hasConstructor, hasInitialize, hasInject, className, accessibilityString, obsoleteText);
                AddDefaultInstance(stringBuilder, hasConstructor, hasInitialize, hasInject, className, accessibilityString, obsoleteText);
                AddDefaultMethod(stringBuilder, hasConstructor, hasInitialize, hasInject, className, accessibilityString, obsoleteText);

                stringBuilder.Append(@"
    }
}
");
                context.AddSource($"ManualDiGeneratedExtensions.{className}.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
            }
        }
        
        private static bool IsSymbolObsolete(ISymbol typeSymbol)
        {
            return typeSymbol.GetAttributes()
                .Any(x => x.AttributeClass?.ToDisplayString() == typeof(ObsoleteAttribute).FullName);
        }
        
        private static bool IsSymbolInjectValid(IPropertySymbol propertySymbol)
        {
            // Check if the property has the Inject attribute
            bool hasInjectAttribute = propertySymbol.GetAttributes()
                .Any(x => x.AttributeClass?.ToDisplayString() == "ManualDi.Main.InjectAttribute");

            if (!hasInjectAttribute)
            {
                return false;
            }

            // Check if the property is public or internal
            bool isPropertyAccessible = propertySymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;

            if (!isPropertyAccessible)
            {
                return false;
            }

            // Check if the setter is public or internal
            return propertySymbol.SetMethod?.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;
        }

        private string GetAccessibilityString(Accessibility accessibility)
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

        private static bool AddFromConstructor(StringBuilder stringBuilder, string className,
            INamedTypeSymbol classSymbol, string accessibilityString, string obsoleteText)
        {
            var constructor = classSymbol
                .Constructors
                .SingleOrDefault(c => c.DeclaredAccessibility == Accessibility.Public);

            if (constructor is null)
            {
                return false;
            }

            var arguments = string.Join(",\r\n                ", constructor.Parameters.Select(p => $"c.Resolve<{FullyQualifyType(p.Type)}>()"));

            stringBuilder.Append($@"
        {obsoleteText}{accessibilityString} static TypeBinding<T, {className}> FromConstructor<T>(this TypeBinding<T, {className}> typeBinding)
        {{
            typeBinding.FromMethod(static c => new {className}({arguments}));
            return typeBinding;
        }}
");
            return true;
        }

        private static bool AddInitialize(StringBuilder stringBuilder, string className, INamedTypeSymbol classSymbol,
            string accessibilityString,
            string obsoleteText)
        {
            var initializeMethod = classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .SingleOrDefault(m => m is { Name: "Initialize", DeclaredAccessibility: Accessibility.Public, IsStatic: false });

            if (initializeMethod is null)
            {
                return false;
            }

            var arguments = string.Join(",\r\n                ", initializeMethod.Parameters.Select(p => $"c.Resolve<{FullyQualifyType(p.Type)}>()"));

            stringBuilder.Append($@"
        {obsoleteText}{accessibilityString} static TypeBinding<T, {className}> Initialize<T>(this TypeBinding<T, {className}> typeBinding)
        {{
            typeBinding.Initialize(static (o, c) => o.Initialize({arguments}));
            return typeBinding;
        }}
");
            return true;
        }

        private static bool AddInject(StringBuilder stringBuilder, string className, INamedTypeSymbol classSymbol,
            string accessibilityString,
            string obsoleteText)
        {
            var injectMethod = classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .SingleOrDefault(m => m is { Name: "Inject", DeclaredAccessibility: Accessibility.Public, IsStatic: false });

            var injectProperties = classSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(IsSymbolInjectValid)
                .ToArray();

            if (injectMethod is null && injectProperties.Length == 0)
            {
                return false;
            }
            
            stringBuilder.AppendLine($@"
        {obsoleteText}{accessibilityString} static TypeBinding<T, {className}> Inject<T>(this TypeBinding<T, {className}> typeBinding)
        {{
            typeBinding.Inject(static (o, c) => 
            {{");

            foreach (var injectProperty in injectProperties)
            {
                stringBuilder.AppendLine($"                o.{injectProperty.Name} = c.Resolve<{FullyQualifyType(injectProperty.Type)}>();");
            }
            
            if (injectMethod is not null)
            {
                var arguments = string.Join(",\r\n                ", injectMethod.Parameters.Select(p => $"c.Resolve<{FullyQualifyType(p.Type)}>()"));
                stringBuilder.AppendLine($"                o.Inject({arguments});");
            }
            
            stringBuilder.Append(@"            });
            return typeBinding;
        }
");
            return true;
        }

        private static void AddDefaultConstructor(StringBuilder stringBuilder, bool hasConstructor, bool hasInitialize,
            bool hasInject, string className, string accessibilityString, string obsoleteText)
        {
            stringBuilder.AppendLine($@"
        {obsoleteText}{accessibilityString} static TypeBinding<T, {className}> Default<T>(this TypeBinding<T, {className}> typeBinding)
        {{");

            if (hasConstructor)
            {
                stringBuilder.AppendLine("            typeBinding.FromConstructor();");
            }

            if (hasInitialize)
            {
                stringBuilder.AppendLine("            typeBinding.Initialize();");
            }

            if (hasInject)
            {
                stringBuilder.AppendLine("            typeBinding.Inject();");
            }

            stringBuilder.AppendLine("            return typeBinding;\r\n        }");
        }
        
        private static void AddDefaultInstance(StringBuilder stringBuilder, bool hasConstructor, bool hasInitialize,
            bool hasInject, string className, string accessibilityString, string obsoleteText)
        {
            stringBuilder.AppendLine($@"
        {obsoleteText}{accessibilityString} static TypeBinding<T, {className}> Default<T>(this TypeBinding<T, {className}> typeBinding, {className} instance)
        {{");

            if (hasConstructor)
            {
                stringBuilder.AppendLine("            typeBinding.FromInstance(instance);");
            }

            if (hasInitialize)
            {
                stringBuilder.AppendLine("            typeBinding.Initialize();");
            }

            if (hasInject)
            {
                stringBuilder.AppendLine("            typeBinding.Inject();");
            }

            stringBuilder.AppendLine("            return typeBinding;\r\n        }");
        }
        
        private static void AddDefaultMethod(StringBuilder stringBuilder, bool hasConstructor, bool hasInitialize,
            bool hasInject, string className, string accessibilityString, string obsoleteText)
        {
            stringBuilder.AppendLine($@"
        {obsoleteText}{accessibilityString} static TypeBinding<T, {className}> Default<T>(this TypeBinding<T, {className}> typeBinding, CreateDelegate<{className}> func)
        {{");

            if (hasConstructor)
            {
                stringBuilder.AppendLine("            typeBinding.FromMethod(func);");
            }

            if (hasInitialize)
            {
                stringBuilder.AppendLine("            typeBinding.Initialize();");
            }

            if (hasInject)
            {
                stringBuilder.AppendLine("            typeBinding.Inject();");
            }

            stringBuilder.AppendLine("            return typeBinding;\r\n        }");
        }

        private static readonly SymbolDisplayFormat FullyQualifyTypeSymbolDisplayFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
        );
        
        private static string FullyQualifyType(ITypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString(FullyQualifyTypeSymbolDisplayFormat);
        }
    }
}
