using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Neos.SourceGenerator.Generator;

[Generator]
public class BuilderGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new BuilderAttributeReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // we can retrieve the populated instance via the context
        BuilderAttributeReceiver syntaxReceiver = (BuilderAttributeReceiver)context.SyntaxReceiver;
        if (syntaxReceiver is null) return;
        // get the recorded mapper class
        foreach (var cds in syntaxReceiver.ClassesWithBuilder)
        {
            var builderDef = GetBuilderDef(cds);
            var builderSource = SourceText.From(builderDef, Encoding.UTF8);
            context.AddSource($"{cds.Identifier}.Builder.cs", builderSource);
        }
    }

    private static string GetBuilderDef(ClassDeclarationSyntax cds)
    {
        var classNamespace = cds.Parent as FileScopedNamespaceDeclarationSyntax;

        return @$"
namespace {classNamespace?.Name.ToString()};
public partial class {cds.Identifier}{{

    public class Builder{{
        private {cds.Identifier} _entity = new {cds.Identifier}();
        public {cds.Identifier} Get{cds.Identifier}() => _entity;
        {GenerateWithMethods(cds)}
    }}
}}";
    }

    private static string GenerateWithMethods(ClassDeclarationSyntax cds)
    {
        return string.Join("\n", cds.Members.OfType<PropertyDeclarationSyntax>().Select(p =>
            $@"
        public Builder With{p.Identifier}({p.Type.ToString()} value){{
            _entity.{p.Identifier}=value;
            return this;
        }}"
        ));
    }

    class BuilderAttributeReceiver : ISyntaxReceiver
    {
        public IImmutableSet<ClassDeclarationSyntax> ClassesWithBuilder => _classesWithBuilder.ToImmutableHashSet();

        private readonly HashSet<ClassDeclarationSyntax> _classesWithBuilder = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is AttributeSyntax attSyntax && attSyntax.Name.ToString() == "Builder"
                                                        && attSyntax.Parent?.Parent is ClassDeclarationSyntax cds)
            {
                _classesWithBuilder.Add(cds);
            }
        }
    }
}