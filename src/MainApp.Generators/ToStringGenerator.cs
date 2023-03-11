using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MainApp.Generators
{
    [Generator]
    public class ToStringGenerator: IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classes = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, _) => IsSyntaxTarget(node),
                transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node);

            context.RegisterSourceOutput(classes, (ctx, source) =>
                Execute(ctx,source)
            );

            context.RegisterPostInitializationOutput(
                static (ctx) => PostInitializationOutput(ctx));
        }

        private static bool IsSyntaxTarget(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDeclarationSyntax
                   && classDeclarationSyntax.AttributeLists.Count > 0;
        }

        private static void PostInitializationOutput(IncrementalGeneratorPostInitializationContext ctx)
        {
            ctx.AddSource("MainApp.Generators.GenerateToStringAttribute.g.cs",
                @"namespace MainApp.Generators
{
    internal class GenerateToStringAttribute : System.Attribute { }
}");
        }

        private void Execute(SourceProductionContext context, ClassDeclarationSyntax classDeclarationSyntax)
        {
            if (classDeclarationSyntax.Parent
                is BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax)
            {
                var namespaceName = namespaceDeclarationSyntax.Name.ToString();
                var className = classDeclarationSyntax.Identifier.Text;
                var fileName = $"{namespaceName}.{className}.g.cs";

                var stringBuilder = new StringBuilder();
                stringBuilder.Append($@"namespace {namespaceName}
{{
    partial class {className}
    {{
         public override string ToString()
        {{
                        return $""");

            var first = true;
            foreach (var memberDeclarationSyntax in classDeclarationSyntax.Members)
            {{
                if (memberDeclarationSyntax
                    is PropertyDeclarationSyntax propertyDeclarationSyntax
                    && propertyDeclarationSyntax.Modifiers.Any(SyntaxKind.PublicKeyword))
                {{
                    if (first)
                    {{
                        first = false;
                    }}
                    else
                    {{
                        stringBuilder.Append("; ");
                    }}
                    var propertyName = propertyDeclarationSyntax.Identifier.Text;
                    stringBuilder.Append($"{propertyName}:{{{propertyName}}}");
                }}
            }}

            stringBuilder.Append($@""";
        }}
    }}
}}
");

                context.AddSource(fileName, stringBuilder.ToString());
            }
        }
    }
}
