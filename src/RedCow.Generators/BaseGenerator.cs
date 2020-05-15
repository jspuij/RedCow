// <copyright file="BaseGenerator.cs" company="Jan-Willem Spuij">
// Copyright 2020 Jan-Willem Spuij
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
// the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace RedCow.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CodeGeneration.Roslyn;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    /// <summary>
    /// Base class for RedCow Generators.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class BaseGenerator : IRichCodeGenerator
    {
        /// <summary>
        /// Create the syntax tree representing the expansion of some member to which this attribute is applied.
        /// </summary>
        /// <param name="context">All the inputs necessary to perform the code generation.</param>
        /// <param name="progress">A way to report diagnostic messages.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The generated member syntax to be added to the project.</returns>
        public abstract Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken);

        /// <summary>
        /// Create additions to compilation unit representing the expansion of some node to which this attribute is applied.
        /// </summary>
        /// <param name="context">All the inputs necessary to perform the code generation.</param>
        /// <param name="progress">A way to report diagnostic messages.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The generated syntax nodes to be added to the compilation unit added to the project.</returns>
        public async Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var node = context.ProcessingNode;

            var result = new RichGenerationResult
            {
                Members = context.ProcessingNode.Ancestors().Aggregate(await this.GenerateAsync(context, progress, cancellationToken), WrapInAncestor),
                Usings = List(
                    new UsingDirectiveSyntax[]
                    {
                        UsingDirective(
                            IdentifierName("System")),
                        UsingDirective(
                            QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("Reflection"))),
                        UsingDirective(
                            QualifiedName(
                                QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("Collections")),
                                IdentifierName("Generic"))),
                        UsingDirective(
                            QualifiedName(
                                QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("Collections")),
                                IdentifierName("ObjectModel"))),
                        UsingDirective(
                            QualifiedName(
                                QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("Diagnostics")),
                                IdentifierName("CodeAnalysis"))),
                        UsingDirective(
                            IdentifierName("RedCow")),
                        UsingDirective(
                            QualifiedName(
                                IdentifierName("RedCow"),
                                IdentifierName("Immutable"))),
                        UsingDirective(
                            QualifiedName(
                                QualifiedName(
                                IdentifierName("RedCow"),
                                IdentifierName("Immutable")),
                                IdentifierName("Collections"))),
                    }),
            };

            return result;
        }

        /// <summary>
        /// Gets the public instance properties for the specified type.
        /// </summary>
        /// <param name="typeSymbol">The type symbol to use.</param>
        /// <returns>The list of properties.</returns>
        protected static IEnumerable<IPropertySymbol> GetPublicInstanceProperties(ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers().
                Where(x => x is IPropertySymbol prop &&
                !prop.IsStatic &&
                !prop.IsIndexer &&
                (prop.DeclaredAccessibility == Accessibility.Internal || prop.DeclaredAccessibility == Accessibility.Public)).
                Cast<IPropertySymbol>();
        }

        /// <summary>
        /// Wraps these members in their ancestor namespace.
        /// </summary>
        /// <param name="generatedMembers">The generate members.</param>
        /// <param name="ancestor">The ancestor node.</param>
        /// <returns>A new syntaxlist.</returns>
        private static SyntaxList<MemberDeclarationSyntax> WrapInAncestor(SyntaxList<MemberDeclarationSyntax> generatedMembers, SyntaxNode ancestor)
        {
            switch (ancestor)
            {
                case NamespaceDeclarationSyntax ancestorNamespace:
                    generatedMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                        CopyAsAncestor(ancestorNamespace)
                        .WithMembers(generatedMembers));
                    break;
                case ClassDeclarationSyntax nestingClass:
                    generatedMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                        CopyAsAncestor(nestingClass)
                        .WithMembers(generatedMembers));
                    break;
                case StructDeclarationSyntax nestingStruct:
                    generatedMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                        CopyAsAncestor(nestingStruct)
                        .WithMembers(generatedMembers));
                    break;
            }

            return generatedMembers;
        }

        /// <summary>
        /// Copy a namespace as ancestor.
        /// </summary>
        /// <param name="syntax">The declaration syntax.</param>
        /// <returns>The copied declaration syntax.</returns>
        private static NamespaceDeclarationSyntax CopyAsAncestor(NamespaceDeclarationSyntax syntax)
        {
            return SyntaxFactory.NamespaceDeclaration(syntax.Name.WithoutTrivia())
                .WithExterns(SyntaxFactory.List(syntax.Externs.Select(x => x.WithoutTrivia())))
                .WithUsings(SyntaxFactory.List(syntax.Usings.Select(x => x.WithoutTrivia())));
        }

        /// <summary>
        /// Copy a class as ancestor.
        /// </summary>
        /// <param name="syntax">The declaration syntax.</param>
        /// <returns>The copied declaration syntax.</returns>
        private static ClassDeclarationSyntax CopyAsAncestor(ClassDeclarationSyntax syntax)
        {
            return SyntaxFactory.ClassDeclaration(syntax.Identifier.WithoutTrivia())
                .WithModifiers(SyntaxFactory.TokenList(syntax.Modifiers.Select(x => x.WithoutTrivia())))
                .WithTypeParameterList(syntax.TypeParameterList);
        }

        /// <summary>
        /// Copy a struct as ancestor.
        /// </summary>
        /// <param name="syntax">The declaration syntax.</param>
        /// <returns>The copied declaration syntax.</returns>
        private static StructDeclarationSyntax CopyAsAncestor(StructDeclarationSyntax syntax)
        {
            return SyntaxFactory.StructDeclaration(syntax.Identifier.WithoutTrivia())
                .WithModifiers(SyntaxFactory.TokenList(syntax.Modifiers.Select(x => x.WithoutTrivia())))
                .WithTypeParameterList(syntax.TypeParameterList);
        }
    }
}
