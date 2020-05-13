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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
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

            ClassDeclarationSyntax partial = this.GeneratePartial(context, applyToClass);

            ClassDeclarationSyntax proxy = this.GenerateProxy(context, applyToClass);

            return Task.FromResult(List(new MemberDeclarationSyntax[] { partial, proxy }));
        }

        /// <summary>
        /// Creates a backinf field based on the readonly interface property.
        /// </summary>
        /// <param name="context">The transformation context.</param>
        /// <param name="property">The property to generate the Getter and Setter for.</param>
        /// <returns>An <see cref="MemberDeclarationSyntax"/>.</returns>
        private static MemberDeclarationSyntax CreateField(TransformationContext context, IPropertySymbol property)
        {
            string documentationText = property.Type.SpecialType == SpecialType.System_Boolean ? $" A boolean indicating whether {property.Name} is true." : $"{property.Name} field.";

            var baseType = GetMutableType(context, property.Type);

            var method = $@"
                /// <summary>
                /// {documentationText}
                /// </summary>
                private {baseType} _{property.Name};
            ";

            return ParseMemberDeclaration(method)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Creates a property with getter and setter based on the readonly interface property.
        /// </summary>
        /// <param name="context">The transformation context.</param>
        /// <param name="property">The property to generate the Getter and Setter for.</param>
        /// <returns>An <see cref="MemberDeclarationSyntax"/>.</returns>
        private static MemberDeclarationSyntax CreateProperty(TransformationContext context, IPropertySymbol property)
        {
            string documentationText = property.Type.SpecialType == SpecialType.System_Boolean ? $" Gets or sets a value indicating whether {property.Name} is true." : $" Gets or sets {property.Name}.";

            var baseType = GetMutableType(context, property.Type);

            var method = $@"
                /// <summary>
                /// {documentationText}
                /// </summary>
                public virtual {baseType} {property.Name}
                {{
                    get => this._{property.Name};
                    set
                    {{
                        if (this.locked)
                        {{
                            throw new ImmutableException(this, $""{property.Name} cannot be changed as this object is immutable"");
                        }}
                        this._{property.Name} = value;
                    }}
                }}
            ";

            return ParseMemberDeclaration(method)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Gets the BaseType (if available).
        /// </summary>
        /// <param name="context">The transformation context.</param>
        /// <param name="typeSymbol">The type symbol to use.</param>
        /// <returns>The base type.</returns>
        private static ITypeSymbol GetMutableType(TransformationContext context, ITypeSymbol typeSymbol)
        {
            ITypeSymbol current = typeSymbol;

            if (current is INamedTypeSymbol namedType)
            {
                if (namedType.IsGenericType)
                {
                    var unbound = namedType.ConstructUnboundGenericType();

                    if (unbound.MetadataName == typeof(System.Collections.Generic.IReadOnlyCollection<>).Name)
                    {
                        var genericArgument = namedType.TypeArguments[0];
                        var mutableType = GetMutableType(context, genericArgument);
                        var iListType = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1");
                        return iListType.Construct(mutableType);
                    }
                }
            }

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
        /// Creates a proxy property with getter and setter based on the readonly interface property.
        /// </summary>
        /// <param name="context">The transformation context.</param>
        /// <param name="property">The property to generate the Getter and Setter for.</param>
        /// <returns>An <see cref="MemberDeclarationSyntax"/>.</returns>
        private static MemberDeclarationSyntax CreateProxyProperty(TransformationContext context, IPropertySymbol property)
        {
            string documentationText = property.Type.SpecialType == SpecialType.System_Boolean ? $" Gets or sets a value indicating whether {property.Name} is true." : $" Gets or sets {property.Name}.";

            var baseType = GetMutableType(context, property.Type);

            var method = $@"
            /// <summary>
            /// {documentationText}
            /// </summary>
            public override {baseType} {property.Name}
            {{
                get
                {{
                    if (this.draftState == null)
                    {{
                        return base.{property.Name};
                    }}

                    return this.draftState.Get<{baseType}>(nameof({property.Name}), () => base.{property.Name}, () => this.Original.{property.Name}, value => base.{property.Name} = value);
                }}
                set
                {{
                    if (this.draftState == null)
                    {{
                        base.{property.Name} = value;
                    }}

                    this.draftState.Set<{baseType}>(nameof({property.Name}), () => base.{property.Name} = value, ((IDraft)this).Clone);
                }}
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
        /// Generates the Locked field on the immutable class.
        /// </summary>
        /// <returns>The property declaration.</returns>
        private static MemberDeclarationSyntax GenerateLockedField()
        {
            var field = $@"
                /// <summary>
                /// Whether the instance is locked.
                /// </summary>
                private bool locked;
            ";

            return ParseMemberDeclaration(field)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the Lock method on the immutable class.
        /// </summary>
        /// <returns>The method declaration.</returns>
        private static MemberDeclarationSyntax GenerateLockMethod(TransformationContext context, IEnumerable<IPropertySymbol> properties)
        {
            var methodStart = $@"
                /// <summary>
                /// Locks the immutable.
                /// </summary>
                void ILockable.Lock()
                {{
                    this.locked = true;
            ";

            var stringBuilder = new StringBuilder(methodStart);

            foreach (var property in properties)
            {
                var baseType = GetMutableType(context, property.Type);

                if (!SymbolEqualityComparer.Default.Equals(baseType, property.Type))
                {
                    stringBuilder.AppendLine($@"
                        if (this.{property.Name} is ILockable && !((ILockable)this.{property.Name}).Locked)
                        {{
                            ((ILockable)this.{property.Name}).Lock();
                        }}");
                }
            }

            stringBuilder.AppendLine("}");

            return ParseMemberDeclaration(stringBuilder.ToString())
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the Locked property on the immutable class.
        /// </summary>
        /// <returns>The property declaration.</returns>
        private static MemberDeclarationSyntax GenerateLockedProperty()
        {
            var property = $@"
                /// <summary>
                /// Gets a value indicating whether the immutable is locked.
                /// </summary>
                bool ILockable.Locked => this.locked;
            ";

            return ParseMemberDeclaration(property)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Creates a property with getter and setter based on the readonly interface property.
        /// </summary>
        /// <param name="context">The transformation context.</param>
        /// <param name="property">The property to generate the Getter and Setter for.</param>
        /// <returns>An <see cref="MemberDeclarationSyntax"/>.</returns>
        private MemberDeclarationSyntax CreateInterfaceProperty(TransformationContext context, IPropertySymbol property)
        {
            string documentationText = property.Type.SpecialType == SpecialType.System_Boolean ? $"Gets a value indicating whether {property.Name} is true." : $"Gets {property.Name}.";

            var baseType = GetMutableType(context, property.Type);

            if (SymbolEqualityComparer.Default.Equals(baseType, property.Type))
            {
                return null;
            }

            string method = string.Empty;

            if (property.Type is INamedTypeSymbol namedType)
            {
                if (namedType.IsGenericType)
                {
                    var unbound = namedType.ConstructUnboundGenericType();

                    if (unbound.MetadataName == typeof(System.Collections.Generic.IReadOnlyCollection<>).Name)
                    {
                        var genericArgument = namedType.TypeArguments[0];
                        var mutableType = GetMutableType(context, genericArgument);
                        method = $@"
                            /// <summary>
                            /// {documentationText}
                            /// </summary>
                            {property.Type} {this.interfaceType.Name}.{property.Name} => new ReadOnlyCollection<{mutableType.Name}>(this.{property.Name});                        ";
                    }
                }
            }

            if (string.IsNullOrEmpty(method))
            {
                method = $@"
                /// <summary>
                /// {documentationText}
                /// </summary>
                {property.Type} {this.interfaceType.Name}.{property.Name} => this.{property.Name};
                ";
            }

            return ParseMemberDeclaration(method)
                .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the partial part of the class.
        /// </summary>
        /// <param name="context">The transformation context.</param>
        /// <param name="sourceClassDeclaration">The source class declaration.</param>
        /// <returns>A partial class declaration.</returns>
        private ClassDeclarationSyntax GeneratePartial(TransformationContext context, ClassDeclarationSyntax sourceClassDeclaration)
        {
            var result = ClassDeclaration(sourceClassDeclaration.Identifier)
                            .AddModifiers(sourceClassDeclaration.Modifiers.ToArray())
                             .AddBaseListTypes(
                                SimpleBaseType(ParseTypeName(this.interfaceType.Name)),
                                SimpleBaseType(ParseTypeName("ILockable")));

            result = result.AddMembers(
                this.interfaceType.GetMembers().
                Where(x => x is IPropertySymbol).
                Cast<IPropertySymbol>().SelectMany(p =>
                {
                    var field = CreateField(context, p);
                    var prop = CreateProperty(context, p);
                    var intProp = this.CreateInterfaceProperty(context, p);
                    return intProp == null ? new[] { field, prop } : new[] { field, prop, intProp };
                }).ToArray());

            result = result.WithAttributeLists(
            List(
                new AttributeListSyntax[]
                {
                    AttributeList(
                        SingletonSeparatedList(
                            Attribute(
                                IdentifierName("ProxyType"))
                            .WithArgumentList(
                                AttributeArgumentList(
                                    SingletonSeparatedList(
                                        AttributeArgument(
                                            TypeOfExpression(
                                                IdentifierName($"Proxy{sourceClassDeclaration.Identifier}")))))))),
                    AttributeList(
                        SingletonSeparatedList(
                            Attribute(
                                IdentifierName("ExcludeFromCodeCoverage")))),
                }));

            result = result.AddMembers(
             GenerateLockedField(),
             GenerateLockedProperty(),
             GenerateLockMethod(
                context,
                this.interfaceType.GetMembers().
                Where(x => x is IPropertySymbol).
                Cast<IPropertySymbol>()));

            return result;
        }

        /// <summary>
        /// Generates the proxy class.
        /// </summary>
        /// <param name="context">The transformation context.</param>
        /// <param name="sourceClassDeclaration">The source class declaration.</param>
        /// <returns>A partial class declaration.</returns>
        private ClassDeclarationSyntax GenerateProxy(TransformationContext context, ClassDeclarationSyntax sourceClassDeclaration)
        {
            var result = ClassDeclaration($"Proxy{sourceClassDeclaration.Identifier}")
                            .WithModifiers(sourceClassDeclaration.Modifiers)
                             .WithAttributeLists(
                                SingletonList<AttributeListSyntax>(
                                    AttributeList(
                                        SingletonSeparatedList<AttributeSyntax>(
                                            Attribute(
                                                IdentifierName("ExcludeFromCodeCoverage"))))
                                    .WithOpenBracketToken(
                                        Token(
                                            GenerateXmlDoc($"Proxy Implementation of <see cref=\"{sourceClassDeclaration.Identifier}\"/>."),
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
                    return CreateProxyProperty(context, p);
                }).ToArray());

            if (sourceClassDeclaration.Members.Any(x => x is ConstructorDeclarationSyntax))
            {
                result = result.WithMembers(List(
                    sourceClassDeclaration.Members.
                    Where(x => x is ConstructorDeclarationSyntax).
                    Cast<ConstructorDeclarationSyntax>().
                    Select(c => this.GenerateProxyConstructor(sourceClassDeclaration, c))));
            }

            result = result.AddMembers(
              this.GenerateDraftStateField(),
              this.GenerateDraftStateProperty(),
              this.GenerateOriginalProperty(sourceClassDeclaration),
              this.GenerateImmutableOriginalProperty(),
              this.GenerateCloneMethod(context, this.interfaceType.GetMembers().
                Where(x => x is IPropertySymbol).
                Cast<IPropertySymbol>()));

            return result;
        }

        /// <summary>
        /// Generates the clone method.
        /// </summary>
        /// <param name="context">The transformation context.</param>
        /// <param name="properties">The properties to generate the clone method for.</param>
        /// <returns>The Member Declaration syntax.</returns>
        private MemberDeclarationSyntax GenerateCloneMethod(TransformationContext context, IEnumerable<IPropertySymbol> properties)
        {
            string methodStart = $@"
        /// <summary>
        /// Clones the object from the original.
        /// </summary>
        void IDraft.Clone()
        {{";

            var stringBuilder = new StringBuilder(methodStart);

            foreach (var property in properties)
            {
                var baseType = GetMutableType(context, property.Type);

                if (SymbolEqualityComparer.Default.Equals(baseType, property.Type))
                {
                    stringBuilder.AppendLine($@"this.{property.Name} = this.Original.{property.Name};");
                }
                else
                {
                    stringBuilder.AppendLine($@"
                        if (!this.{property.Name}.IsDraft())
                        {{
                            this.{property.Name} = this.Original.{property.Name};
                        }}");
                }
            }

            stringBuilder.AppendLine("}");
            return ParseMemberDeclaration(stringBuilder.ToString())
    .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates a single proxy constructor.
        /// </summary>
        /// <param name="sourceClassDeclaration">The source class declaration.</param>
        /// <param name="constructorDeclarationSyntax">An optional existing constructor on the base class.</param>
        /// <returns>A member declaration.</returns>
        private MemberDeclarationSyntax GenerateProxyConstructor(
            ClassDeclarationSyntax sourceClassDeclaration,
            ConstructorDeclarationSyntax constructorDeclarationSyntax = null)
        {
            SeparatedSyntaxList<ParameterSyntax> parameters =
                constructorDeclarationSyntax?.ParameterList?.Parameters
                ?? default(SeparatedSyntaxList<ParameterSyntax>);

            string arguments = parameters.Any() ? parameters.ToFullString() : string.Empty;
            var constructor = $@"
                /// <summary>
                /// Initializes a new instance of the <see cref=""Proxy{sourceClassDeclaration.Identifier}""/> class.
                /// </summary>
                public Proxy{sourceClassDeclaration.Identifier}({arguments}) : base({string.Join(",", parameters.Select(x => x.Identifier))})
                {{
                }}
            ";

            return ParseMemberDeclaration(constructor)
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
        /// Generates the draft state field.
        /// </summary>
        /// <returns>A member declaration.</returns>
        private MemberDeclarationSyntax GenerateDraftStateField()
        {
            var field = $@"
                /// <summary>
                /// the draftState field.
                /// </summary>
                private ObjectDraftState draftState;
            ";

            return ParseMemberDeclaration(field)
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
                    set => this.draftState = (ObjectDraftState)value;
                }}
            ";

            return ParseMemberDeclaration(property)
                .NormalizeWhitespace();
        }
    }
}
