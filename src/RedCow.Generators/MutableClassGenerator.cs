// <copyright file="MutableClassGenerator.cs" company="Jan-Willem Spuij">
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
    public class MutableClassGenerator : BaseGenerator, IRichCodeGenerator
    {
        /// <summary>
        /// The type of the Immutable Interface.
        /// </summary>
        private readonly INamedTypeSymbol interfaceType;

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
        public override Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            // Our generator is applied to any class that our attribute is applied to.
            var applyToClass = (ClassDeclarationSyntax)context.ProcessingNode;

            ClassDeclarationSyntax partial = this.GeneratePartial(applyToClass);

            ClassDeclarationSyntax immutable = this.GenerateImmutable(applyToClass);

            ClassDeclarationSyntax draft = this.GenerateDraft(applyToClass);

            partial = partial.WithAttributeLists(
            List(
                new AttributeListSyntax[]
                {
                    AttributeList(
                        SingletonSeparatedList(
                            Attribute(
                                IdentifierName("ImmutableType"))
                            .WithArgumentList(
                                AttributeArgumentList(
                                    SingletonSeparatedList(
                                        AttributeArgument(
                                            TypeOfExpression(
                                                IdentifierName("ImmutableTestPerson")))))))),
                    AttributeList(
                        SingletonSeparatedList(
                            Attribute(
                                IdentifierName("DraftType"))
                            .WithArgumentList(
                                AttributeArgumentList(
                                    SingletonSeparatedList(
                                        AttributeArgument(
                                            TypeOfExpression(
                                                IdentifierName("DraftTestPerson")))))))),
                }));

            return Task.FromResult(List(new MemberDeclarationSyntax[] { partial, immutable, draft }));
        }

        /// <summary>
        /// Creates a property with getter and setter based on the readonly interface property.
        /// </summary>
        /// <param name="p">The property to generate the Getter and Setter for.</param>
        /// <returns>An <see cref="PropertyDeclarationSyntax"/>.</returns>
        private static PropertyDeclarationSyntax CreateProperty(IPropertySymbol p)
        {
            string documentationText = p.Type.SpecialType == SpecialType.System_Boolean ? $" Gets or sets a value indicating whether {p.Name} is true." : $" Gets or sets {p.Name}.";

            return PropertyDeclaration(ParseTypeName(p.Type.Name), p.Name)
                    .WithModifiers(
                        TokenList(
                            new[]
                            {
                Token(
                    GenerateXmlDoc(documentationText),
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
        /// Creates a property with getter and setter that throws an <see cref="InvalidOperationException"/>,
        /// based on the readonly interface property.
        /// </summary>
        /// <param name="p">The property to generate the Getter and Setter for.</param>
        /// <returns>An <see cref="PropertyDeclarationSyntax"/>.</returns>
        private static PropertyDeclarationSyntax CreateImmutableProperty(IPropertySymbol p)
        {
            string documentationText = p.Type.SpecialType == SpecialType.System_Boolean ? $" Gets or sets a value indicating whether {p.Name} is true." : $" Gets or sets {p.Name}.";

            return PropertyDeclaration(ParseTypeName(p.Type.Name), p.Name)
                    .WithModifiers(
                        TokenList(
                            new[]
                            {
                Token(
                    GenerateXmlDoc(documentationText),
                    SyntaxKind.PublicKeyword,
                    TriviaList()),
                Token(SyntaxKind.OverrideKeyword),
                            }))
        .WithAccessorList(
            AccessorList(
                List(
                    new AccessorDeclarationSyntax[]
                    {
                        AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBody(
                            ArrowExpressionClause(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    BaseExpression(),
                                    IdentifierName(p.Name))))
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(
                            SyntaxKind.SetAccessorDeclaration)
                        .WithExpressionBody(
                            ArrowExpressionClause(
                                ThrowExpression(
                                    ObjectCreationExpression(
                                        IdentifierName("InvalidOperationException"))
                                    .WithArgumentList(
                                        ArgumentList()))))
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken)),
                    }))).NormalizeWhitespace();
        }

        /// <summary>
        /// Generate the XML Documentation for the Property.
        /// </summary>
        /// <param name="documentationText">The documentation text.</param>
        /// <returns>The XML Documentation as <see cref="SyntaxTriviaList"/>.</returns>
        private static SyntaxTriviaList GenerateXmlDoc(string documentationText)
        {
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

        /// <summary>
        /// Generates the partial part of the class.
        /// </summary>
        /// <param name="sourceClassDeclaration">The source class declaration.</param>
        /// <returns>A partial class declaration.</returns>
        private ClassDeclarationSyntax GeneratePartial(ClassDeclarationSyntax sourceClassDeclaration)
        {
            var result = ClassDeclaration(sourceClassDeclaration.Identifier)
                            .AddModifiers(sourceClassDeclaration.Modifiers.ToArray());

            result = result.AddMembers(
                this.interfaceType.GetMembers().
                Where(x => x is IPropertySymbol).
                Cast<IPropertySymbol>().Select(p =>
                {
                    return CreateProperty(p);
                }).ToArray());
            return result;
        }

        /// <summary>
        /// Generates the draft class.
        /// </summary>
        /// <param name="sourceClassDeclaration">The source class declaration.</param>
        /// <returns>A partial class declaration.</returns>
        private ClassDeclarationSyntax GenerateDraft(ClassDeclarationSyntax sourceClassDeclaration)
        {
            var modifiers = sourceClassDeclaration.Modifiers.ToList();
            modifiers[0] = Token(
                GenerateXmlDoc($"Draft Implementation of <see cref=\"{sourceClassDeclaration.Identifier}\"/>."),
                modifiers[0].Kind(),
                TriviaList());

            var result = ClassDeclaration($"Draft{sourceClassDeclaration.Identifier}")
                            .AddModifiers(modifiers.ToArray())
                            .AddBaseListTypes(SimpleBaseType(ParseTypeName(sourceClassDeclaration.Identifier.Text)), SimpleBaseType(ParseTypeName($"IDraft<{sourceClassDeclaration.Identifier.Text}>")));

            return result;
        }

        /// <summary>
        /// Generates the immutable derived class.
        /// </summary>
        /// <param name="sourceClassDeclaration">The source class declaration.</param>
        /// <returns>A partial class declaration.</returns>
        private ClassDeclarationSyntax GenerateImmutable(ClassDeclarationSyntax sourceClassDeclaration)
        {
            var modifiers = sourceClassDeclaration.Modifiers.ToList();
            modifiers[0] = Token(
                GenerateXmlDoc($"Immutable Implementation of <see cref=\"{sourceClassDeclaration.Identifier}\"/>."),
                modifiers[0].Kind(),
                TriviaList());

            var result = ClassDeclaration($"Immutable{sourceClassDeclaration.Identifier}")
                            .AddModifiers(modifiers.ToArray())
                            .AddBaseListTypes(SimpleBaseType(ParseTypeName(sourceClassDeclaration.Identifier.Text)), SimpleBaseType(ParseTypeName(this.interfaceType.Name)));

            result = result.AddMembers(
                this.interfaceType.GetMembers().
                Where(x => x is IPropertySymbol).
                Cast<IPropertySymbol>().Select(p =>
                {
                    return CreateImmutableProperty(p);
                }).ToArray());
            return result;
        }
    }
}
