using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Text;

namespace BuildDateGenerator;

[Generator(LanguageNames.CSharp)]
public partial class BuildDateGenerator : IIncrementalGenerator
{
    private static readonly string namespaceName = "BuildDateGenerator";
    private static readonly string buildDateAttribute = "GenerateBuildDateAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate Attribute
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource($"{namespaceName}.{buildDateAttribute}.cs", $$"""
namespace {{namespaceName}};

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class {{buildDateAttribute}} : Attribute
{
}
""");
        });

        // SyntaxProvider
        var syntax = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"{namespaceName}.{buildDateAttribute}",
            static (node, token) => true,
            static (context, token) => context)
            .Collect();
        context.RegisterSourceOutput(syntax, GenerateSource);
    }


    public static void GenerateSource(
        SourceProductionContext context,
        ImmutableArray<GeneratorAttributeSyntaxContext> sources)
    {
        var currentDateTime = DateTimeOffset.Now;
        foreach (var source in sources)
        {
            var attributeSyntaxContext = source;
            if (attributeSyntaxContext.TargetSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                // Namespace
                var sourceNamespace = namedTypeSymbol.ContainingNamespace.IsGlobalNamespace
                    ? string.Empty
                    : namedTypeSymbol.ContainingNamespace.ToString();
                // ClassName
                var sourceType = namedTypeSymbol.Name;
                // Source
                context.AddSource($"{sourceNamespace}.{sourceType}.cs", $$"""
{{(string.IsNullOrEmpty(sourceNamespace) ? string.Empty : $"namespace {sourceNamespace};")}}
            
partial class {{sourceType}}
{
    /// <summary>
    /// BuildDate
    /// {{currentDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz")}}
    /// </summary>
    public static readonly DateTimeOffset BuildDate = new DateTimeOffset({{currentDateTime.Ticks}}, TimeSpan.FromTicks({{currentDateTime.Offset.Ticks}}));
}
""");
            }
        }
    }
}