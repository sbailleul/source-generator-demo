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
        // Debugger.Launch();
        context.RegisterForSyntaxNotifications(() => new BuilderAttributeReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // we can retrieve the populated instance via the context
        BuilderAttributeReceiver syntaxReceiver = (BuilderAttributeReceiver)context.SyntaxReceiver;
        if (syntaxReceiver is null) return;
        // get the recorded mapper class
        foreach (var tds in syntaxReceiver.ClassesWithBuilder)
        {
            var builderDef = GetBuilderDef(tds);
            var builderSource = SourceText.From(builderDef, Encoding.UTF8);
            context.AddSource($"{tds.Identifier}.Builder.cs", builderSource);
        }
    }

    private static string GetBuilderDef(TypeDeclarationSyntax tds)
    {
        var classNamespace = tds.Parent as FileScopedNamespaceDeclarationSyntax;
        return @$"
namespace {classNamespace?.Name.ToString()};
{tds.Modifiers.ToFullString()} {GetTypeName(tds)} {tds.Identifier}{{

    public class Builder{{
        private {tds.Identifier} _entity = new {tds.Identifier}();
        public {tds.Identifier} Get{tds.Identifier}() => _entity;
        {GenerateWithMethods(tds)}
    }}
}}";
    }

    private static string GetTypeName(TypeDeclarationSyntax tds)
    {
        return tds switch
        {
            ClassDeclarationSyntax => "class",
            InterfaceDeclarationSyntax => "interface",
            RecordDeclarationSyntax => "record",
            _ => throw new InvalidOperationException($"{tds.Kind()} is not handled by builder generator")
        };
    }

    private static string GenerateWithMethods(TypeDeclarationSyntax tds)
    {
        return string.Join("\n", tds.Members.OfType<PropertyDeclarationSyntax>().Select(p =>
            $@"
        public Builder With{p.Identifier}({p.Type.ToString()} value){{
            _entity.{p.Identifier}=value;
            return this;
        }}"
        ));
    }

    class BuilderAttributeReceiver : ISyntaxReceiver
    {
        public IImmutableSet<TypeDeclarationSyntax> ClassesWithBuilder => _classesWithBuilder.ToImmutableHashSet();

        private readonly HashSet<TypeDeclarationSyntax> _classesWithBuilder = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is AttributeSyntax attSyntax && attSyntax.Name.ToString() == "Builder"
                                                        && attSyntax.Parent?.Parent is TypeDeclarationSyntax tds)
            {
                    _classesWithBuilder.Add(tds);
            }
        }
    }
}