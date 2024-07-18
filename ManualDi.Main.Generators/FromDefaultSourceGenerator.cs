using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ManualDi.Main.Generators
{
    [Generator]
    public class FromDefaultSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            var methods = new List<string>();

            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

                if (classSymbol is null || classSymbol.DeclaredAccessibility != Accessibility.Public)
                    continue;

                var publicConstructors = classSymbol.Constructors.Where(c => c.DeclaredAccessibility == Accessibility.Public).ToList();

                if (publicConstructors.Count != 1)
                    continue;

                var constructor = publicConstructors.Single();
                var arguments = string.Join(",\n", constructor.Parameters.Select(p => $"c.Resolve<{FullyQualifyType(p.Type)}>()"));
                var className = FullyQualifyType(classSymbol);

                var methodSource = $@"
        public static TypeBinding<T, {className}> FromDefault<T>(this TypeBinding<T, {className}> typeBinding)
            where {className} : T
        {{
            typeBinding.FromMethod(static c => new {className}(
                {arguments}));
            return typeBinding;
        }}
";

                methods.Add(methodSource);
            }

            var methodsSource = string.Join("", methods);
            
            var formattedAssemblyName = Regex.Replace(context.Compilation.AssemblyName ?? string.Empty, "[^A-Za-z0-9_]", "");
            var source = $@"
using ManualDi.Main;

namespace ManualDi.Main
{{
    public static class ManualDiFromDefaultExtensions_{formattedAssemblyName}
    {{
{methodsSource}
    }}
}}
";

            context.AddSource($"ManualDiFromDefaultExtensions.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private static string FullyQualifyType(ITypeSymbol typeSymbol)
        {
            return typeSymbol.ContainingNamespace.IsGlobalNamespace ? typeSymbol.Name : $"{typeSymbol.ContainingNamespace}.{typeSymbol.Name}";
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclaration)
                {
                    CandidateClasses.Add(classDeclaration);
                }
            }
        }
    }
}
