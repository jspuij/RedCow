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
    /// Generates a Mutable class.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
                                                IdentifierName($"Immutable{applyToClass.Identifier}")))))))),
                    AttributeList(
                        SingletonSeparatedList(
                            Attribute(
                                IdentifierName("DraftType"))
                            .WithArgumentList(
                                AttributeArgumentList(
                                    SingletonSeparatedList(
                                        AttributeArgument(
                                            TypeOfExpression(
                                                IdentifierName($"Draft{applyToClass.Identifier}")))))))),
                }));

            return Task.FromResult(List(new MemberDeclarationSyntax[] { partial, immutable, draft }));
        }

        /// <summary>
        /// Creates a property with getter and setter based on the readonly interface property.
        /// </summary>
        /// <param name="p">The property to generate the Getter and Setter for.</param>
        /// <returns>An <see cref="MemberDeclarationSyntax"/>.</returns>
        private static MemberDeclarationSyntax CreateProperty(IPropertySymbol p)
        {
            string documentationText = p.Type.SpecialType == SpecialType.System_Boolean ? $" Gets or sets a value indicating whether {p.Name} is true." : $" Gets or sets {p.Name}.";

            var baseType = GetBaseType(p.Type);

            var method = $@"
                /// <summary>
                /// {documentationText}
                /// </summary>
                public virtual {baseType} {p.Name}
                {{
                    get;
                    set;
                }}
            ";

            return ParseMemberDeclaration(method)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Gets the BaseType (if available).
        /// </summary>
        /// <param name="typeSymbol">The type symbol to use.</param>
        /// <returns>The base type.</returns>
        private static ITypeSymbol GetBaseType(ITypeSymbol typeSymbol)
        {
            ITypeSymbol current = typeSymbol;

            while (current != null)
            {
                foreach (var attribute in current.GetAttributes())
                {
                    if (attribute.AttributeClass.Name == "GenerateMutableAttribute")
                    {
                        return current;
                    }

                    if (attribute.AttributeClass.Name == "GenerateProducersAttribute")
                    {
                        return (ITypeSymbol)attribute.ConstructorArguments[0].Value;
                    }
                }

                current = current.BaseType;
            }

            return typeSymbol;
        }

        /// <summary>
        /// Creates an immutable property with getter and setter that throws an <see cref="InvalidOperationException"/>,
        /// based on the readonly interface property.
        /// </summary>
        /// <param name="p">The property to generate the Getter and Setter for.</param>
        /// <returns>An <see cref="MemberDeclarationSyntax"/>.</returns>
        private static MemberDeclarationSyntax CreateImmutableProperty(IPropertySymbol p)
        {
            string documentationText = p.Type.SpecialType == SpecialType.System_Boolean ? $" Gets or sets a value indicating whether {p.Name} is true." : $" Gets or sets {p.Name}.";

            var baseType = GetBaseType(p.Type);

            var method = $@"
                /// <summary>
                /// {documentationText}
                /// </summary>
                public override {baseType} {p.Name}
                {{
                    get => base.{p.Name};
                    set
                    {{
                        if (this.Locked)
                        {{
                            throw new ImmutableException(this, ""This is an immutable object, and cannot be changed."");
                        }}
                        
                        base.{p.Name} = value;
                    }}
                }}
            ";

            return ParseMemberDeclaration(method)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Creates a draft property with getter and setter based on the readonly interface property.
        /// </summary>
        /// <param name="p">The property to generate the Getter and Setter for.</param>
        /// <returns>An <see cref="MemberDeclarationSyntax"/>.</returns>
        private static MemberDeclarationSyntax CreateDraftProperty(IPropertySymbol p)
        {
            string documentationText = p.Type.SpecialType == SpecialType.System_Boolean ? $" Gets or sets a value indicating whether {p.Name} is true." : $" Gets or sets {p.Name}.";

            var baseType = GetBaseType(p.Type);

            var method = $@"
                /// <summary>
                ///  {documentationText}
                /// </summary>
                public override {baseType} {p.Name}
                {{
                    get => this.draftState.Get<{baseType}>(nameof({p.Name}), () => base.{p.Name}, value => base.{p.Name} = value);
                    set => this.draftState.Set<{baseType}>(nameof({p.Name}), () => base.{p.Name} = value);
                }}
            ";

            return ParseMemberDeclaration(method)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generate the XML Documentation for the Property.
        /// </summary>
        /// <param name="documentationText">The documentation text.</param>
        /// <returns>The XML Documentation as <see cref="SyntaxTriviaList"/>.</returns>
        private static SyntaxTriviaList GenerateXmlDoc(string documentationText)
        {
            var trivia = $@"
                /// <summary>
                /// {documentationText}
                /// </summary>
            ";
            return ParseLeadingTrivia(trivia);
        }

        /// <summary>
        /// Creates a property with getter and setter based on the readonly interface property.
        /// </summary>
        /// <param name="p">The property to generate the Getter and Setter for.</param>
        /// <returns>An <see cref="MemberDeclarationSyntax"/>.</returns>
        private MemberDeclarationSyntax CreateInterfaceProperty(IPropertySymbol p)
        {
            string documentationText = p.Type.SpecialType == SpecialType.System_Boolean ? $" Gets or sets a value indicating whether {p.Name} is true." : $" Gets or sets {p.Name}.";

            var baseType = GetBaseType(p.Type);

            if (SymbolEqualityComparer.Default.Equals(baseType, p.Type))
            {
                return null;
            }

            var method = $@"
                /// <summary>
                /// {documentationText}
                /// </summary>
                {p.Type} {this.interfaceType.Name}.{p.Name} => this.{p.Name};
            ";

            return ParseMemberDeclaration(method)
                .NormalizeWhitespace();
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
                Cast<IPropertySymbol>().SelectMany(p =>
                {
                    var prop = CreateProperty(p);
                    var intProp = this.CreateInterfaceProperty(p);
                    return intProp == null ? new[] { prop } : new[] { prop, intProp };
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
            var result = ClassDeclaration($"Draft{sourceClassDeclaration.Identifier}")
                            .WithModifiers(sourceClassDeclaration.Modifiers)
                             .WithAttributeLists(
                                SingletonList<AttributeListSyntax>(
                                    AttributeList(
                                        SingletonSeparatedList<AttributeSyntax>(
                                            Attribute(
                                                IdentifierName("ExcludeFromCodeCoverage"))))
                                    .WithOpenBracketToken(
                                        Token(
                                            GenerateXmlDoc($"Draft Implementation of <see cref=\"{sourceClassDeclaration.Identifier}\"/>."),
                                            SyntaxKind.OpenBracketToken,
                                            TriviaList()))))
                            .AddBaseListTypes(
                                SimpleBaseType(ParseTypeName(sourceClassDeclaration.Identifier.Text)),
                                SimpleBaseType(ParseTypeName($"IDraft<{sourceClassDeclaration.Identifier.Text}>")));

            result = result.AddMembers(
                this.interfaceType.GetMembers().
                Where(x => x is IPropertySymbol).
                Cast<IPropertySymbol>().Select(p =>
                {
                    return CreateDraftProperty(p);
                }).ToArray());

            if (sourceClassDeclaration.Members.Any(x => x is ConstructorDeclarationSyntax))
            {
                result = result.WithMembers(List(
                    sourceClassDeclaration.Members.
                    Where(x => x is ConstructorDeclarationSyntax).
                    Cast<ConstructorDeclarationSyntax>().
                    Select(c => this.GenerateDraftConstructor(sourceClassDeclaration, c))));
            }
            else
            {
                result = result.AddMembers(this.GenerateDraftConstructor(sourceClassDeclaration));
            }

            result = result.AddMembers(
              this.GenerateDraftStateField(),
              this.GenerateDraftStateProperty(),
              this.GenerateOriginalProperty(sourceClassDeclaration),
              this.GenerateImmutableOriginalProperty());

            return result;
        }

        /// <summary>
        /// Generates a single draft constructor.
        /// </summary>
        /// <param name="sourceClassDeclaration">The source class declaration.</param>
        /// <param name="constructorDeclarationSyntax">An optional existing constructor on the base class.</param>
        /// <returns>A member declaration.</returns>
        private MemberDeclarationSyntax GenerateDraftConstructor(
            ClassDeclarationSyntax sourceClassDeclaration,
            ConstructorDeclarationSyntax constructorDeclarationSyntax = null)
        {
            SeparatedSyntaxList<ParameterSyntax> parameters =
                constructorDeclarationSyntax?.ParameterList?.Parameters
                ?? default(SeparatedSyntaxList<ParameterSyntax>);

            string arguments = parameters.Any() ? $", {parameters.ToFullString()}" : string.Empty;
            var constructor = $@"
                /// <summary>
                /// Initializes a new instance of the <see cref=""Draft{sourceClassDeclaration.Identifier}""/> class.
                /// </summary>
                public Draft{sourceClassDeclaration.Identifier}(DraftState draftState{arguments}) : base({string.Join(",", parameters.Select(x => x.Identifier))})
                {{
                    this.draftState = draftState ?? throw new ArgumentNullException(nameof(draftState));
                }}
            ";

            return ParseMemberDeclaration(constructor)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the draft state field.
        /// </summary>
        /// <returns>A member declaration.</returns>
        private MemberDeclarationSyntax GenerateDraftStateField()
        {
            var field = $@"
                /// <summary>
                /// the draftState field.
                /// </summary>
                private DraftState draftState;
            ";

            return ParseMemberDeclaration(field)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the Original property.
        /// </summary>
        /// <param name="sourceClassDeclaration">The source class declaration.</param>
        /// <returns>A member declaration.</returns>
        private MemberDeclarationSyntax GenerateOriginalProperty(ClassDeclarationSyntax sourceClassDeclaration)
        {
            var property = $@"
                /// <summary>
                /// Gets the original.
                /// </summary>
                public {sourceClassDeclaration.Identifier} Original => this.draftState.GetOriginal<{sourceClassDeclaration.Identifier}>();
            ";

            return ParseMemberDeclaration(property)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the ImmutableOriginal property.
        /// </summary>
        /// <returns>A member declaration.</returns>
        private MemberDeclarationSyntax GenerateImmutableOriginalProperty()
        {
            var property = $@"
                /// <summary>
                /// Gets the original as Immutable.
                /// </summary>
                public {this.interfaceType.Name} ImmutableOriginal => this.draftState.GetOriginal<{this.interfaceType.Name}>();
            ";

            return ParseMemberDeclaration(property)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the DraftState property.
        /// </summary>
        /// <returns>A member declaration.</returns>
        private MemberDeclarationSyntax GenerateDraftStateProperty()
        {
            var property = $@"
                /// <summary>
                /// Gets or sets the DraftState.
                /// </summary>
                DraftState IDraft.DraftState
                {{
                    get => this.draftState;
                }}
            ";

            return ParseMemberDeclaration(property)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the immutable derived class.
        /// </summary>
        /// <param name="sourceClassDeclaration">The source class declaration.</param>
        /// <returns>A partial class declaration.</returns>
        private ClassDeclarationSyntax GenerateImmutable(ClassDeclarationSyntax sourceClassDeclaration)
        {
            var result = ClassDeclaration($"Immutable{sourceClassDeclaration.Identifier}")
                            .WithModifiers(sourceClassDeclaration.Modifiers)
                             .WithAttributeLists(
                                SingletonList<AttributeListSyntax>(
                                    AttributeList(
                                        SingletonSeparatedList<AttributeSyntax>(
                                            Attribute(
                                                IdentifierName("ExcludeFromCodeCoverage"))))
                                    .WithOpenBracketToken(
                                        Token(
                                            GenerateXmlDoc($"Immutable Implementation of <see cref=\"{sourceClassDeclaration.Identifier}\"/>."),
                                            SyntaxKind.OpenBracketToken,
                                            TriviaList()))))
                            .AddBaseListTypes(
                                SimpleBaseType(ParseTypeName(sourceClassDeclaration.Identifier.Text)),
                                SimpleBaseType(ParseTypeName($"ILockable")));

            result = result.WithMembers(List(
                   sourceClassDeclaration.Members.
                   Where(x => x is ConstructorDeclarationSyntax).
                   Cast<ConstructorDeclarationSyntax>().
                   Select(c => this.GenerateImmutableConstructor(sourceClassDeclaration, c))));

            result = result.AddMembers(
                this.interfaceType.GetMembers().
                Where(x => x is IPropertySymbol).
                Cast<IPropertySymbol>().Select(p =>
                {
                    return CreateImmutableProperty(p);
                }).ToArray());

            result = result.AddMembers(
                this.GenerateLockedProperty(),
                this.GenerateLockMethod());

            return result;
        }

        /// <summary>
        /// Generates a single immutable constructor.
        /// </summary>
        /// <param name="sourceClassDeclaration">The source class declaration.</param>
        /// <param name="constructorDeclarationSyntax">An optional existing constructor on the base class.</param>
        /// <returns>A member declaration.</returns>
        private MemberDeclarationSyntax GenerateImmutableConstructor(ClassDeclarationSyntax sourceClassDeclaration, ConstructorDeclarationSyntax constructorDeclarationSyntax)
        {
            SeparatedSyntaxList<ParameterSyntax> parameters =
                            constructorDeclarationSyntax?.ParameterList?.Parameters
                            ?? default(SeparatedSyntaxList<ParameterSyntax>);

            var constructor = $@"
                /// <summary>
                /// Initializes a new instance of the <see cref=""Immutable{sourceClassDeclaration.Identifier}""/> class.
                /// </summary>
                public Immutable{sourceClassDeclaration.Identifier}({parameters.ToFullString()}) : base({string.Join(",", parameters.Select(x => x.Identifier))})
                {{
                }}
            ";

            return ParseMemberDeclaration(constructor)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the Lock method on the immutable class.
        /// </summary>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateLockMethod()
        {
            var method = $@"
                /// <summary>
                /// Locks the immutable.
                /// </summary>
                public void Lock()
                {{
                    this.Locked = true;
                }}
            ";

            return ParseMemberDeclaration(method)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the Locked property on the immutable class.
        /// </summary>
        /// <returns>The property declaration.</returns>
        private MemberDeclarationSyntax GenerateLockedProperty()
        {
            var property = $@"
                /// <summary>
                /// Gets a value indicating whether the immutable is locked.
                /// </summary>
                public bool Locked {{ get; private set; }} = false;
            ";

            return ParseMemberDeclaration(property)
                .NormalizeWhitespace();
        }
    }
}
