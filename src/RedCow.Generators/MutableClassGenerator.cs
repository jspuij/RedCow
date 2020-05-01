﻿// <copyright file="MutableClassGenerator.cs" company="Jan-Willem Spuij">
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CodeGeneration.Roslyn;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    /// <summary>
    /// Generates a Mutable class.
    /// </summary>
    public class MutableClassGenerator : ICodeGenerator
    {
        /// <summary>
        /// The type of the Immutable Interface.
        /// </summary>
        private INamedTypeSymbol interfaceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MutableClassGenerator"/> class.
        /// </summary>
        /// <param name="attributeData">The attribute data to use.</param>
        public MutableClassGenerator(AttributeData attributeData)
        {
            this.interfaceType = (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value;

            // System.Diagnostics.Debugger.Launch();
            // while (!System.Diagnostics.Debugger.IsAttached)
            // {
            //     Thread.Sleep(500); // eww, eww, eww
            // }
        }

        /// <summary>
        /// Generates the code.
        /// </summary>
        /// <param name="context">The transformation context.</param>
        /// <param name="progress">Progress information.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            // Our generator is applied to any class that our attribute is applied to.
            var applyToClass = (ClassDeclarationSyntax)context.ProcessingNode;

            var modifiers = applyToClass.Modifiers.ToList();
            modifiers.Insert(1, Token(SyntaxKind.AbstractKeyword));

            var copy = ClassDeclaration(applyToClass.Identifier)
                .AddModifiers(modifiers.ToArray())
                .AddBaseListTypes(SimpleBaseType(ParseTypeName(this.interfaceType.Name)));

            copy = copy.AddMembers(
                this.interfaceType.GetMembers().
                Where(x => x is IPropertySymbol).
                Cast<IPropertySymbol>().Select(p =>
                {
                    return CreateProperty(p);
                }).ToArray());

            return Task.FromResult(SingletonList<MemberDeclarationSyntax>(copy));
        }

        /// <summary>
        /// Creates a property with getter and setter based on the readonly interface property.
        /// </summary>
        /// <param name="p">The property to generate the Getter and Setter for.</param>
        /// <returns>An <see cref="PropertyDeclarationSyntax"/>.</returns>
        private static PropertyDeclarationSyntax CreateProperty(IPropertySymbol p)
        {
            return PropertyDeclaration(ParseTypeName(p.Type.Name), p.Name)
                    .WithModifiers(
                        TokenList(
                            new[]
                            {
                Token(
                    GenerateXmlDocForProperty(p),
                    SyntaxKind.PublicKeyword,
                    TriviaList()),
                Token(SyntaxKind.VirtualKeyword),
                            }))
                .WithAccessorList(
                        AccessorList(
                            List(
                                new AccessorDeclarationSyntax[]
                                {
                    AccessorDeclaration(
                        SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(
                        SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken)),
                                })))
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generate the XML Documentation for the Property.
        /// </summary>
        /// <param name="p">The property info.</param>
        /// <returns>The XML Documentation as <see cref="SyntaxTriviaList"/>.</returns>
        private static SyntaxTriviaList GenerateXmlDocForProperty(IPropertySymbol p)
        {
            string documentationText = p.Type.SpecialType == SpecialType.System_Boolean ? $" Gets or sets a value indicating whether {p.Name} is true." : $" Gets or sets {p.Name}.";
            return TriviaList(
                    Trivia(
                        DocumentationCommentTrivia(
                        SyntaxKind.SingleLineDocumentationCommentTrivia,
                        List(
                            new XmlNodeSyntax[]
                            {
                    XmlText()
                    .WithTextTokens(
                        TokenList(
                            XmlTextLiteral(
                                TriviaList(
                                    DocumentationCommentExterior("///")),
                                " ",
                                " ",
                                TriviaList()))),
                    XmlExampleElement(
                        SingletonList(
                            (XmlNodeSyntax)XmlText()
                            .WithTextTokens(
                                TokenList(
                                    new[]
                                    {
                                        XmlTextNewLine(
                                            TriviaList(),
                                            Environment.NewLine,
                                            Environment.NewLine,
                                            TriviaList()),
                                        XmlTextLiteral(
                                            TriviaList(
                                                DocumentationCommentExterior("///")),
                                            documentationText,
                                            documentationText,
                                            TriviaList()),
                                        XmlTextNewLine(
                                            TriviaList(),
                                            Environment.NewLine,
                                            Environment.NewLine,
                                            TriviaList()),
                                        XmlTextLiteral(
                                            TriviaList(
                                                DocumentationCommentExterior("///")),
                                            " ",
                                            " ",
                                            TriviaList()),
                                    }))))
                    .WithStartTag(
                        XmlElementStartTag(
                            XmlName(
                                Identifier("summary"))))
                    .WithEndTag(
                        XmlElementEndTag(
                            XmlName(
                                Identifier("summary")))),
                    XmlText()
                    .WithTextTokens(
                        TokenList(
                            XmlTextNewLine(
                                TriviaList(),
                                Environment.NewLine,
                                Environment.NewLine,
                                TriviaList()))),
                            }))));
        }
    }
}
