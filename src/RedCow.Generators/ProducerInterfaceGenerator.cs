// <copyright file="ProducerInterfaceGenerator.cs" company="Jan-Willem Spuij">
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
    /// Generates producer methods on the Immutable interface.
    /// </summary>
    public class ProducerInterfaceGenerator : ICodeGenerator
    {
        /// <summary>
        /// The type of the Mutable.
        /// </summary>
        private readonly INamedTypeSymbol mutableType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerInterfaceGenerator"/> class.
        /// </summary>
        /// <param name="attributeData">The attribute data to use.</param>
        public ProducerInterfaceGenerator(AttributeData attributeData)
        {
            this.mutableType = (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value;

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
            var applyToInterface = (InterfaceDeclarationSyntax)context.ProcessingNode;

            InterfaceDeclarationSyntax partial = this.GeneratePartial(applyToInterface);

            return Task.FromResult(SingletonList<MemberDeclarationSyntax>(partial));
        }

        /// <summary>
        /// Generates a partial for the interface.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface to apply the partial to.</param>
        /// <returns>The interface declaration syntax.</returns>
        private InterfaceDeclarationSyntax GeneratePartial(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var result = InterfaceDeclaration(interfaceDeclaration.Identifier)
                .AddModifiers(interfaceDeclaration.Modifiers.ToArray());

            result = result.AddMembers(
                this.GenerateInitialProduce(interfaceDeclaration),
                this.GenerateProduceAction(interfaceDeclaration),
                this.GenerateStaticProduceAction(interfaceDeclaration),
                this.GenerateProduceFunction(interfaceDeclaration),
                this.GenerateStaticProduceFunction(interfaceDeclaration),
                this.GenerateProducerAction(interfaceDeclaration),
                this.GenerateProducerFunction(interfaceDeclaration),
                this.GenerateProducerActionWithArgument(interfaceDeclaration),
                this.GenerateProducerFunctionWithArgument(interfaceDeclaration));
            return result;
        }

        /// <summary>
        /// Generates the produce function for initial state.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateInitialProduce(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return MethodDeclaration(
                IdentifierName(interfaceDeclaration.Identifier),
                Identifier("Produce"))
            .WithModifiers(
                TokenList(
                    new[]
                    {
                        Token(
                            this.GenerateProduceDocumentation(interfaceDeclaration, true, false, false),
                            SyntaxKind.PublicKeyword,
                            TriviaList()),
                        Token(SyntaxKind.StaticKeyword),
                    }))
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Parameter(
                                Identifier("initialState"))
                            .WithType(
                                IdentifierName(this.mutableType.Name)),
                            Token(SyntaxKind.CommaToken),
                            Parameter(
                                Identifier("cloneProvider"))
                            .WithType(
                                QualifiedName(
                                    IdentifierName("RedCow"),
                                    IdentifierName("ICloneProvider")))
                            .WithDefault(
                                EqualsValueClause(
                                    LiteralExpression(
                                        SyntaxKind.NullLiteralExpression))),
                        })))
            .WithExpressionBody(
                ArrowExpressionClause(
                    InvocationExpression(
                        IdentifierName("Produce"))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(
                                        IdentifierName("initialState")),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        SimpleLambdaExpression(
                                            Parameter(
                                                Identifier("p")),
                                            Block())),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        IdentifierName("cloneProvider")),
                                })))))
            .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken))
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the produce function that accepts an action.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProduceAction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return
            MethodDeclaration(
                IdentifierName(interfaceDeclaration.Identifier),
                Identifier("Produce"))
            .WithModifiers(
                TokenList(
                    Token(
                        this.GenerateProduceDocumentation(interfaceDeclaration, false, false, true),
                        SyntaxKind.PublicKeyword,
                        TriviaList())))
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Parameter(
                                Identifier("producer"))
                            .WithType(
                                GenericName(
                                    Identifier("Action"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName(this.mutableType.Name))))),
                            Token(SyntaxKind.CommaToken),
                            Parameter(
                                Identifier("cloneProvider"))
                            .WithType(
                                 QualifiedName(
                                    IdentifierName("RedCow"),
                                    IdentifierName("ICloneProvider")))
                            .WithDefault(
                                EqualsValueClause(
                                    LiteralExpression(
                                        SyntaxKind.NullLiteralExpression))),
                        })))
            .WithExpressionBody(
                ArrowExpressionClause(
                    InvocationExpression(
                        IdentifierName("Produce"))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(
                                        CastExpression(
                                            IdentifierName(this.mutableType.Name),
                                            ThisExpression())),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        IdentifierName("producer")),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        IdentifierName("cloneProvider")),
                                })))))
            .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken))
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the static produce function that accepts an action.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateStaticProduceAction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return MethodDeclaration(
                IdentifierName(interfaceDeclaration.Identifier),
                Identifier("Produce"))
            .WithModifiers(
                TokenList(
                    new[]
                    {
                        Token(
                            this.GenerateProduceDocumentation(interfaceDeclaration, true, false, true),
                            SyntaxKind.PublicKeyword,
                            TriviaList()),
                        Token(SyntaxKind.StaticKeyword),
                    }))
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Parameter(
                                Identifier("initialState"))
                            .WithType(
                                IdentifierName(this.mutableType.Name)),
                            Token(SyntaxKind.CommaToken),
                            Parameter(
                                Identifier("producer"))
                            .WithType(
                                GenericName(
                                    Identifier("Action"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName(this.mutableType.Name))))),
                            Token(SyntaxKind.CommaToken),
                            Parameter(
                                Identifier("cloneProvider"))
                            .WithType(
                                 QualifiedName(
                                    IdentifierName("RedCow"),
                                    IdentifierName("ICloneProvider")))
                            .WithDefault(
                                EqualsValueClause(
                                    LiteralExpression(
                                        SyntaxKind.NullLiteralExpression))),
                        })))
            .WithExpressionBody(
                ArrowExpressionClause(
                    InvocationExpression(
                        InvocationExpression(
                            IdentifierName("Producer"))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        Argument(
                                            IdentifierName("producer")),
                                        Token(SyntaxKind.CommaToken),
                                        Argument(
                                            IdentifierName("cloneProvider")),
                                    }))))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    IdentifierName("initialState")))))))
            .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken))
    .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the produce function that accepts a function.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProduceFunction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return MethodDeclaration(
                IdentifierName(interfaceDeclaration.Identifier),
                Identifier("Produce"))
            .WithModifiers(
                TokenList(
                    Token(
                        this.GenerateProduceDocumentation(interfaceDeclaration, false, true, true),
                        SyntaxKind.PublicKeyword,
                        TriviaList())))
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Parameter(
                                Identifier("producer"))
                            .WithType(
                                GenericName(
                                    Identifier("Func"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName(this.mutableType.Name))))),
                            Token(SyntaxKind.CommaToken),
                            Parameter(
                                Identifier("cloneProvider"))
                            .WithType(
                                 QualifiedName(
                                    IdentifierName("RedCow"),
                                    IdentifierName("ICloneProvider")))
                            .WithDefault(
                                EqualsValueClause(
                                    LiteralExpression(
                                        SyntaxKind.NullLiteralExpression))),
                        })))
            .WithExpressionBody(
                ArrowExpressionClause(
                    InvocationExpression(
                        IdentifierName("Produce"))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(
                                        CastExpression(
                                            IdentifierName(this.mutableType.Name),
                                            ThisExpression())),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        IdentifierName("producer")),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        IdentifierName("cloneProvider")),
                                })))))
            .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken))
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the static produce function that accepts a function.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateStaticProduceFunction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return MethodDeclaration(
                IdentifierName(interfaceDeclaration.Identifier),
                Identifier("Produce"))
            .WithModifiers(
                TokenList(
                    new[]
                    {
                        Token(
                            this.GenerateProduceDocumentation(interfaceDeclaration, true, true, true),
                            SyntaxKind.PublicKeyword,
                            TriviaList()),
                        Token(SyntaxKind.StaticKeyword),
                    }))
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Parameter(
                                Identifier("initialState"))
                            .WithType(
                                IdentifierName(this.mutableType.Name)),
                            Token(SyntaxKind.CommaToken),
                            Parameter(
                                Identifier("producer"))
                            .WithType(
                                GenericName(
                                    Identifier("Func"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName(this.mutableType.Name))))),
                            Token(SyntaxKind.CommaToken),
                            Parameter(
                                Identifier("cloneProvider"))
                            .WithType(
                                 QualifiedName(
                                    IdentifierName("RedCow"),
                                    IdentifierName("ICloneProvider")))
                            .WithDefault(
                                EqualsValueClause(
                                    LiteralExpression(
                                        SyntaxKind.NullLiteralExpression))),
                        })))
            .WithExpressionBody(
                ArrowExpressionClause(
                    InvocationExpression(
                        InvocationExpression(
                            IdentifierName("Producer"))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        Argument(
                                            IdentifierName("producer")),
                                        Token(SyntaxKind.CommaToken),
                                        Argument(
                                            IdentifierName("cloneProvider")),
                                    }))))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    IdentifierName("initialState")))))))
            .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken))
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generate the documentation for the produce method.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <param name="isStatic">Whether this is the static produce method.</param>
        /// <param name="usesFunction">Whether the producer uses a function or an action.</param>
        /// <param name="hasProducer">Whether the producer is empty.</param>
        /// <returns>The documentation.</returns>
        private SyntaxTriviaList GenerateProduceDocumentation(InterfaceDeclarationSyntax interfaceDeclaration, bool isStatic, bool usesFunction, bool hasProducer)
        {
            string functionOrAction = usesFunction ? "function" : "action";

            var list = new List<XmlNodeSyntax>()
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
                    XmlText()
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
                                        DocumentationCommentExterior("        ///")),
                                    " Produces the next ",
                                    " Produces the next ",
                                    TriviaList()),
                            })),
                    XmlNullKeywordElement()
                    .WithAttributes(
                        SingletonList<XmlAttributeSyntax>(
                            XmlCrefAttribute(
                                NameMemberCref(
                                    GenericName(
                                        Identifier("Immutable"))
                                    .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SingletonSeparatedList<TypeSyntax>(
                                                IdentifierName("T")))))))),
                    XmlText()
                    .WithTextTokens(
                        TokenList(
                            new[]
                            {
                                XmlTextLiteral(
                                    TriviaList(),
                                    " based on the",
                                    " based on the",
                                    TriviaList()),
                                XmlTextNewLine(
                                    TriviaList(),
                                    Environment.NewLine,
                                    Environment.NewLine,
                                    TriviaList()),
                                XmlTextLiteral(
                                    TriviaList(
                                        DocumentationCommentExterior("        ///")),
                                    hasProducer ? $" specified producer {functionOrAction}." : "intial state.",
                                    hasProducer ? $" specified producer {functionOrAction}." : "intial state.",
                                    TriviaList()),
                                XmlTextNewLine(
                                    TriviaList(),
                                    Environment.NewLine,
                                    Environment.NewLine,
                                    TriviaList()),
                                XmlTextLiteral(
                                    TriviaList(
                                        DocumentationCommentExterior("        ///")),
                                    " ",
                                    " ",
                                    TriviaList()),
                            })))
                .WithStartTag(
                    XmlElementStartTag(
                        XmlName(
                            Identifier("summary"))))
                .WithEndTag(
                    XmlElementEndTag(
                        XmlName(
                            Identifier("summary")))),
            };

            if (isStatic)
            {
                list.Add(
                XmlText()
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
                                    DocumentationCommentExterior("        ///")),
                                " ",
                                " ",
                                TriviaList()),
                        })));
                list.Add(
                XmlExampleElement(
                    SingletonList<XmlNodeSyntax>(
                        XmlText()
                        .WithTextTokens(
                            TokenList(
                                XmlTextLiteral(
                                    TriviaList(),
                                    "The initial State.",
                                    "The initial State.",
                                    TriviaList())))))
                .WithStartTag(
                    XmlElementStartTag(
                        XmlName(
                            Identifier("param")))
                    .WithAttributes(
                        SingletonList<XmlAttributeSyntax>(
                            XmlNameAttribute(
                                XmlName(
                                    Identifier("name")),
                                Token(SyntaxKind.DoubleQuoteToken),
                                IdentifierName("initialState"),
                                Token(SyntaxKind.DoubleQuoteToken)))))
                .WithEndTag(
                    XmlElementEndTag(
                        XmlName(
                            Identifier("param")))));
            }

            if (hasProducer)
            {
                list.Add(
                XmlText()
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
                                    DocumentationCommentExterior("        ///")),
                                " ",
                                " ",
                                TriviaList()),
                        })));
                list.Add(
                XmlExampleElement(
                    SingletonList<XmlNodeSyntax>(
                        XmlText()
                        .WithTextTokens(
                            TokenList(
                                XmlTextLiteral(
                                    TriviaList(),
                                    $"The producer {functionOrAction}.",
                                    $"The producer {functionOrAction}.",
                                    TriviaList())))))
                .WithStartTag(
                    XmlElementStartTag(
                        XmlName(
                            Identifier("param")))
                    .WithAttributes(
                        SingletonList<XmlAttributeSyntax>(
                            XmlNameAttribute(
                                XmlName(
                                    Identifier("name")),
                                Token(SyntaxKind.DoubleQuoteToken),
                                IdentifierName("producer"),
                                Token(SyntaxKind.DoubleQuoteToken)))))
                .WithEndTag(
                    XmlElementEndTag(
                        XmlName(
                            Identifier("param")))));
            }

            list.Add(
            XmlText()
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
                                    DocumentationCommentExterior("        ///")),
                                " ",
                                " ",
                                TriviaList()),
                    })));
            list.Add(
            XmlExampleElement(
                SingletonList<XmlNodeSyntax>(
                    XmlText()
                    .WithTextTokens(
                        TokenList(
                            XmlTextLiteral(
                                TriviaList(),
                                "The clone provider to use.",
                                "The clone provider to use.",
                                TriviaList())))))
            .WithStartTag(
                XmlElementStartTag(
                    XmlName(
                        Identifier("param")))
                .WithAttributes(
                    SingletonList<XmlAttributeSyntax>(
                        XmlNameAttribute(
                            XmlName(
                                Identifier("name")),
                            Token(SyntaxKind.DoubleQuoteToken),
                            IdentifierName("cloneProvider"),
                            Token(SyntaxKind.DoubleQuoteToken)))))
            .WithEndTag(
                XmlElementEndTag(
                    XmlName(
                        Identifier("param")))));
            list.Add(
            XmlText()
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
                                    DocumentationCommentExterior("        ///")),
                                " ",
                                " ",
                                TriviaList()),
                    })));
            list.Add(
            XmlExampleElement(
                SingletonList<XmlNodeSyntax>(
                    XmlText()
                    .WithTextTokens(
                        TokenList(
                            XmlTextLiteral(
                                TriviaList(),
                                "The next immutable state.",
                                "The next immutable state.",
                                TriviaList())))))
            .WithStartTag(
                XmlElementStartTag(
                    XmlName(
                        Identifier("returns"))))
            .WithEndTag(
                XmlElementEndTag(
                    XmlName(
                        Identifier("returns")))));
            list.Add(
        XmlText()
        .WithTextTokens(
            TokenList(
                XmlTextNewLine(
                    TriviaList(),
                    Environment.NewLine,
                    Environment.NewLine,
                    TriviaList()))));

            return TriviaList(
                        Trivia(
                            DocumentationCommentTrivia(
                                SyntaxKind.SingleLineDocumentationCommentTrivia,
                                List(list))));
        }

        /// <summary>
        /// Generates the producer function that accepts a function.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerAction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return MethodDeclaration(
            GenericName(
                Identifier("Func"))
            .WithTypeArgumentList(
                TypeArgumentList(
                    SeparatedList<TypeSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            IdentifierName(this.mutableType.Name),
                            Token(SyntaxKind.CommaToken),
                            IdentifierName(interfaceDeclaration.Identifier),
                        }))),
            Identifier("Producer"))
        .WithModifiers(
            TokenList(
                new[]
                {
                    Token(
                        this.GenerateProducerDocumentation(interfaceDeclaration, false, false),
                        SyntaxKind.PublicKeyword,
                        TriviaList()),
                    Token(SyntaxKind.StaticKeyword),
                }))
        .WithParameterList(
            ParameterList(
                SeparatedList<ParameterSyntax>(
                    new SyntaxNodeOrToken[]
                    {
                        Parameter(
                            Identifier("producer"))
                        .WithType(
                            GenericName(
                                Identifier("Action"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(this.mutableType.Name))))),
                        Token(SyntaxKind.CommaToken),
                        Parameter(
                            Identifier("cloneProvider"))
                        .WithType(
                             QualifiedName(
                                    IdentifierName("RedCow"),
                                    IdentifierName("ICloneProvider")))
                        .WithDefault(
                            EqualsValueClause(
                                LiteralExpression(
                                    SyntaxKind.NullLiteralExpression))),
                    })))
        .WithExpressionBody(
            ArrowExpressionClause(
                ParenthesizedLambdaExpression(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                Identifier("immutable1")))),
                    InvocationExpression(
                        InvocationExpression(
                            GenericName(
                                Identifier("Producer"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        PredefinedType(
                                            Token(SyntaxKind.ObjectKeyword))))))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        Argument(
                                            ParenthesizedLambdaExpression(
                                                ParameterList(
                                                    SeparatedList<ParameterSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                        {
                                                            Parameter(
                                                                Identifier("argument1")),
                                                            Token(SyntaxKind.CommaToken),
                                                            Parameter(
                                                                Identifier("_")),
                                                        })),
                                                InvocationExpression(
                                                    IdentifierName("producer"))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SingletonSeparatedList(
                                                            Argument(
                                                                IdentifierName("argument1"))))))),
                                        Token(SyntaxKind.CommaToken),
                                        Argument(
                                            IdentifierName("cloneProvider")),
                                    }))))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(
                                        IdentifierName("immutable1")),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.NullLiteralExpression)),
                                }))))))
        .WithSemicolonToken(
            Token(SyntaxKind.SemicolonToken))
        .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the producer function that accepts an action.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerFunction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return MethodDeclaration(
            GenericName(
                Identifier("Func"))
            .WithTypeArgumentList(
                TypeArgumentList(
                    SeparatedList<TypeSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            IdentifierName(this.mutableType.Name),
                            Token(SyntaxKind.CommaToken),
                            IdentifierName(interfaceDeclaration.Identifier),
                        }))),
            Identifier("Producer"))
        .WithModifiers(
            TokenList(
                new[]
                {
                    Token(
                        this.GenerateProducerDocumentation(interfaceDeclaration, true, false),
                        SyntaxKind.PublicKeyword,
                        TriviaList()),
                    Token(SyntaxKind.StaticKeyword),
                }))
        .WithParameterList(
            ParameterList(
                SeparatedList<ParameterSyntax>(
                    new SyntaxNodeOrToken[]
                    {
                        Parameter(
                            Identifier("producer"))
                        .WithType(
                            GenericName(
                                Identifier("Func"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(this.mutableType.Name))))),
                        Token(SyntaxKind.CommaToken),
                        Parameter(
                            Identifier("cloneProvider"))
                        .WithType(
                             QualifiedName(
                                    IdentifierName("RedCow"),
                                    IdentifierName("ICloneProvider")))
                        .WithDefault(
                            EqualsValueClause(
                                LiteralExpression(
                                    SyntaxKind.NullLiteralExpression))),
                    })))
        .WithExpressionBody(
            ArrowExpressionClause(
                ParenthesizedLambdaExpression(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                Identifier("immutable1")))),
                    InvocationExpression(
                        InvocationExpression(
                            GenericName(
                                Identifier("Producer"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        PredefinedType(
                                            Token(SyntaxKind.ObjectKeyword))))))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        Argument(
                                            SimpleLambdaExpression(
                                                Parameter(
                                                    Identifier("_")),
                                                InvocationExpression(
                                                    IdentifierName("producer")))),
                                        Token(SyntaxKind.CommaToken),
                                        Argument(
                                            IdentifierName("cloneProvider")),
                                    }))))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(
                                        IdentifierName("immutable1")),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.NullLiteralExpression)),
                                }))))))
        .WithSemicolonToken(
            Token(SyntaxKind.SemicolonToken))
        .NormalizeWhitespace();
        }

        /// <summary>
        /// Generate the producer which accepts an action with a single Argument.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerActionWithArgument(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return MethodDeclaration(
            GenericName(
                Identifier("Func"))
            .WithTypeArgumentList(
                TypeArgumentList(
                    SeparatedList<TypeSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            IdentifierName(this.mutableType.Name),
                            Token(SyntaxKind.CommaToken),
                            IdentifierName("TArg"),
                            Token(SyntaxKind.CommaToken),
                            IdentifierName(interfaceDeclaration.Identifier),
                        }))),
            Identifier("Producer"))
        .WithModifiers(
            TokenList(
                new[]
                {
                    Token(
                        this.GenerateProducerDocumentation(interfaceDeclaration, false, true),
                        SyntaxKind.PublicKeyword,
                        TriviaList()),
                    Token(SyntaxKind.StaticKeyword),
                }))
        .WithTypeParameterList(
            TypeParameterList(
                SingletonSeparatedList(
                    TypeParameter(
                        Identifier("TArg")))))
        .WithParameterList(
            ParameterList(
                SeparatedList<ParameterSyntax>(
                    new SyntaxNodeOrToken[]
                    {
                        Parameter(
                            Identifier("producer"))
                        .WithType(
                            GenericName(
                                Identifier("Action"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SeparatedList<TypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            IdentifierName(this.mutableType.Name),
                                            Token(SyntaxKind.CommaToken),
                                            IdentifierName("TArg"),
                                        })))),
                        Token(SyntaxKind.CommaToken),
                        Parameter(
                            Identifier("cloneProvider"))
                        .WithType(
                             QualifiedName(
                                    IdentifierName("RedCow"),
                                    IdentifierName("ICloneProvider")))
                        .WithDefault(
                            EqualsValueClause(
                                LiteralExpression(
                                    SyntaxKind.NullLiteralExpression))),
                    })))
        .WithExpressionBody(
            ArrowExpressionClause(
                ParenthesizedLambdaExpression(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(
                                    Identifier("immutable1")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(
                                    Identifier("argument1")),
                            })),
                    Block(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                IdentifierName("var"))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                        Identifier("scope"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            InvocationExpression(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName("immutable1"),
                                                    IdentifierName("CreateDraft")))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SeparatedList<ArgumentSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                        {
                                                            Argument(
                                                                DeclarationExpression(
                                                                    IdentifierName("var"),
                                                                    SingleVariableDesignation(
                                                                        Identifier("draft"))))
                                                            .WithRefKindKeyword(
                                                                Token(SyntaxKind.OutKeyword)),
                                                            Token(SyntaxKind.CommaToken),
                                                            Argument(
                                                                IdentifierName("cloneProvider")),
                                                        }))))))))
                        .WithUsingKeyword(
                            Token(SyntaxKind.UsingKeyword)),
                        ExpressionStatement(
                            InvocationExpression(
                                IdentifierName("producer"))
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList<ArgumentSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            Argument(
                                                CastExpression(
                                                    IdentifierName(this.mutableType.Name),
                                                    IdentifierName("draft"))),
                                            Token(SyntaxKind.CommaToken),
                                            Argument(
                                                IdentifierName("argument1")),
                                        })))),
                        ReturnStatement(
                            CastExpression(
                                IdentifierName(interfaceDeclaration.Identifier),
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("scope"),
                                        IdentifierName("FinishDraft")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                CastExpression(
                                                    IdentifierName(this.mutableType.Name),
                                                    IdentifierName("draft"))))))))))))
        .WithSemicolonToken(
            Token(SyntaxKind.SemicolonToken))
.NormalizeWhitespace();
        }

        /// <summary>
        /// Generate the producer which accepts a function with a single Argument.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerFunctionWithArgument(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return MethodDeclaration(
            GenericName(
                Identifier("Func"))
            .WithTypeArgumentList(
                TypeArgumentList(
                    SeparatedList<TypeSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            IdentifierName(this.mutableType.Name),
                            Token(SyntaxKind.CommaToken),
                            IdentifierName("TArg"),
                            Token(SyntaxKind.CommaToken),
                            IdentifierName(interfaceDeclaration.Identifier),
                        }))),
            Identifier("Producer"))
        .WithModifiers(
            TokenList(
                new[]
                {
                    Token(
                        this.GenerateProducerDocumentation(interfaceDeclaration, true, true),
                        SyntaxKind.PublicKeyword,
                        TriviaList()),
                    Token(SyntaxKind.StaticKeyword),
                }))
        .WithTypeParameterList(
            TypeParameterList(
                SingletonSeparatedList<TypeParameterSyntax>(
                    TypeParameter(
                        Identifier("TArg")))))
        .WithParameterList(
            ParameterList(
                SeparatedList<ParameterSyntax>(
                    new SyntaxNodeOrToken[]
                    {
                        Parameter(
                            Identifier("producer"))
                        .WithType(
                            GenericName(
                                Identifier("Func"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SeparatedList<TypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            IdentifierName("TArg"),
                                            Token(SyntaxKind.CommaToken),
                                            IdentifierName(this.mutableType.Name),
                                        })))),
                        Token(SyntaxKind.CommaToken),
                        Parameter(
                            Identifier("cloneProvider"))
                        .WithType(
                             QualifiedName(
                                    IdentifierName("RedCow"),
                                    IdentifierName("ICloneProvider")))
                        .WithDefault(
                            EqualsValueClause(
                                LiteralExpression(
                                    SyntaxKind.NullLiteralExpression))),
                    })))
         .WithExpressionBody(
            ArrowExpressionClause(
                ParenthesizedLambdaExpression(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(
                                    Identifier("immutable1")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(
                                    Identifier("argument1")),
                            })),
                    Block(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                IdentifierName("var"))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                        Identifier("scope"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            InvocationExpression(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName("immutable1"),
                                                    IdentifierName("CreateDraft")))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SeparatedList<ArgumentSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                        {
                                                            Argument(
                                                                DeclarationExpression(
                                                                    IdentifierName("var"),
                                                                    DiscardDesignation()))
                                                            .WithRefKindKeyword(
                                                                Token(SyntaxKind.OutKeyword)),
                                                            Token(SyntaxKind.CommaToken),
                                                            Argument(
                                                                IdentifierName("cloneProvider")),
                                                        }))))))))
                        .WithUsingKeyword(
                            Token(SyntaxKind.UsingKeyword)),
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                IdentifierName(this.mutableType.Name))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                        Identifier("draft"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            InvocationExpression(
                                                IdentifierName("producer"))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SingletonSeparatedList(
                                                        Argument(
                                                            IdentifierName("argument1")))))))))),
                        ReturnStatement(
                            CastExpression(
                                IdentifierName(interfaceDeclaration.Identifier),
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("scope"),
                                        IdentifierName("FinishDraft")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                IdentifierName("draft")))))))))))
          .WithSemicolonToken(
            Token(SyntaxKind.SemicolonToken))
          .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates Method Documentation.
        /// </summary>
        /// <param name="interfaceDeclaration">The immutable interface declaration.</param>
        /// <param name="usesFunction">Whether the producer uses a function or an action.</param>
        /// <param name="hasArgument">Indicates whether the producer has an argument.</param>
        /// <returns>The documentation.</returns>
        private SyntaxTriviaList GenerateProducerDocumentation(InterfaceDeclarationSyntax interfaceDeclaration, bool usesFunction, bool hasArgument)
        {
            string documentationText;

            if (hasArgument)
            {
                if (usesFunction)
                {
                    documentationText = "The producer function that returns an object of type T with a single argument.";
                }
                else
                {
                    documentationText = "The producer action that operates on an object of type T with a single argument.";
                }
            }
            else
            {
                if (usesFunction)
                {
                    documentationText = "The producer function that returns an object of type T.";
                }
                else
                {
                    documentationText = "The producer action that operates on an object of type T.";
                }
            }

            var xmlText = new List<XmlNodeSyntax>
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
                            SingletonList<XmlNodeSyntax>(
                                XmlText()
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
                                                    DocumentationCommentExterior("        ///")),
                                                " Creates a Producer delegate that can be used to curry on an Immutable State.",
                                                " Creates a Producer delegate that can be used to curry on an Immutable State.",
                                                TriviaList()),
                                            XmlTextNewLine(
                                                TriviaList(),
                                                Environment.NewLine,
                                                Environment.NewLine,
                                                TriviaList()),
                                            XmlTextLiteral(
                                                TriviaList(
                                                    DocumentationCommentExterior("        ///")),
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
                                new[]
                                {
                                    XmlTextNewLine(
                                        TriviaList(),
                                        Environment.NewLine,
                                        Environment.NewLine,
                                        TriviaList()),
                                    XmlTextLiteral(
                                        TriviaList(
                                            DocumentationCommentExterior("        ///")),
                                        " ",
                                        " ",
                                        TriviaList()),
                                })),
                XmlExampleElement(
                            SingletonList<XmlNodeSyntax>(
                                XmlText()
                                .WithTextTokens(
                                    TokenList(
                                        XmlTextLiteral(
                                            TriviaList(),
                                            documentationText,
                                            documentationText,
                                            TriviaList())))))
                        .WithStartTag(
                            XmlElementStartTag(
                                XmlName(
                                    Identifier("param")))
                            .WithAttributes(
                                SingletonList<XmlAttributeSyntax>(
                                    XmlNameAttribute(
                                        XmlName(
                                            Identifier("name")),
                                        Token(SyntaxKind.DoubleQuoteToken),
                                        IdentifierName("producer"),
                                        Token(SyntaxKind.DoubleQuoteToken)))))
                        .WithEndTag(
                            XmlElementEndTag(
                                XmlName(
                                    Identifier("param")))),
                XmlText()
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
                                            DocumentationCommentExterior("        ///")),
                                        " ",
                                        " ",
                                        TriviaList()),
                                })),
                XmlExampleElement(
                            SingletonList<XmlNodeSyntax>(
                                XmlText()
                                .WithTextTokens(
                                    TokenList(
                                        XmlTextLiteral(
                                            TriviaList(),
                                            "The clone provider to use.",
                                            "The clone provider to use.",
                                            TriviaList())))))
                        .WithStartTag(
                            XmlElementStartTag(
                                XmlName(
                                    Identifier("param")))
                            .WithAttributes(
                                SingletonList<XmlAttributeSyntax>(
                                    XmlNameAttribute(
                                        XmlName(
                                            Identifier("name")),
                                        Token(SyntaxKind.DoubleQuoteToken),
                                        IdentifierName("cloneProvider"),
                                        Token(SyntaxKind.DoubleQuoteToken)))))
                        .WithEndTag(
                            XmlElementEndTag(
                                XmlName(
                                    Identifier("param")))),
            };

            if (hasArgument)
            {
                xmlText.Add(
                            XmlText()
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
                                            DocumentationCommentExterior("        ///")),
                                        " ",
                                        " ",
                                        TriviaList()),
                                    })));
                xmlText.Add(
                            XmlExampleElement(
                                SingletonList<XmlNodeSyntax>(
                                    XmlText()
                                    .WithTextTokens(
                                        TokenList(
                                            XmlTextLiteral(
                                                TriviaList(),
                                                "The type of the argument.",
                                                "The type of the argument.",
                                                TriviaList())))))
                            .WithStartTag(
                                XmlElementStartTag(
                                    XmlName(
                                        Identifier("typeparam")))
                                .WithAttributes(
                                    SingletonList<XmlAttributeSyntax>(
                                        XmlNameAttribute(
                                            XmlName(
                                                Identifier("name")),
                                            Token(SyntaxKind.DoubleQuoteToken),
                                            IdentifierName("TArg"),
                                            Token(SyntaxKind.DoubleQuoteToken)))))
                            .WithEndTag(
                                XmlElementEndTag(
                                    XmlName(
                                        Identifier("typeparam")))));
            }

            xmlText.Add(
                        XmlText()
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
                                            DocumentationCommentExterior("        ///")),
                                        " ",
                                        " ",
                                        TriviaList()),
                                })));
            xmlText.Add(
                        XmlExampleElement(
                            SingletonList<XmlNodeSyntax>(
                                XmlText()
                                .WithTextTokens(
                                    TokenList(
                                        XmlTextLiteral(
                                            TriviaList(),
                                            "A producer delegate.",
                                            "A producer delegate.",
                                            TriviaList())))))
                        .WithStartTag(
                            XmlElementStartTag(
                                XmlName(
                                    Identifier("returns"))))
                        .WithEndTag(
                            XmlElementEndTag(
                                XmlName(
                                    Identifier("returns")))));
            xmlText.Add(
                        XmlText()
                        .WithTextTokens(
                            TokenList(
                                XmlTextNewLine(
                                    TriviaList(),
                                    Environment.NewLine,
                                    Environment.NewLine,
                                    TriviaList()))));

            return TriviaList(
                    Trivia(
                        DocumentationCommentTrivia(
                            SyntaxKind.SingleLineDocumentationCommentTrivia,
                            List(xmlText))));
        }
    }
}
