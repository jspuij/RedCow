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
    /// Generates producer methods on the Immutable interface.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProducerInterfaceGenerator : BaseGenerator, ICodeGenerator
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
        public override Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
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
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;

            var method = $@"
                /// <summary>
                /// Produces the next <see cref = ""Immutable{{T}}""/> based on the
                /// intial state.
                /// </summary>
                /// <param name = ""initialState"">The initial State.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>The next immutable state.</returns>
                [ExcludeFromCodeCoverage]
                public static {interfaceName} Produce({className} initialState, ICloneProvider cloneProvider = null) =>
                Produce(initialState, p => {{ }}, cloneProvider);
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the produce function that accepts an action.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProduceAction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;

            var method = $@"
                /// <summary>
                /// Produces the next <see cref = ""Immutable{{T}}""/> based on the
                /// specified recipe action.
                /// </summary>
                /// <param name = ""recipe"">The recipe action.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>The next immutable state.</returns>
                [ExcludeFromCodeCoverage]
                public {interfaceName} Produce(Action<{className}> recipe, ICloneProvider cloneProvider = null) => Produce(({className})this, recipe, cloneProvider);
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the static produce function that accepts an action.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateStaticProduceAction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;

            var method = $@"
                /// <summary>
                /// Produces the next <see cref = ""Immutable{{T}}""/> based on the
                /// specified recipe action.
                /// </summary>
                /// <param name = ""initialState"">The initial State.</param>
                /// <param name = ""recipe"">The recipe action.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>The next immutable state.</returns>
                [ExcludeFromCodeCoverage]
                public static {interfaceName} Produce({className} initialState, Action<{className}> recipe, ICloneProvider cloneProvider = null) =>
                    Producer(recipe, cloneProvider)(initialState);
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the produce function that accepts a function.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProduceFunction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;

            var method = $@"
                /// <summary>
                /// Produces the next <see cref = ""Immutable{{T}}""/> based on the
                /// specified recipe function.
                /// </summary>
                /// <param name = ""recipe"">The recipe function.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>The next immutable state.</returns>
                [ExcludeFromCodeCoverage]
                public {interfaceName} Produce(Func<{className}> recipe, ICloneProvider cloneProvider = null) => Produce(({className})this, recipe, cloneProvider);
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the static produce function that accepts a function.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateStaticProduceFunction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;

            var method = $@"
                /// <summary>
                /// Produces the next <see cref = ""Immutable{{T}}""/> based on the
                /// specified recipe function.
                /// </summary>
                /// <param name = ""initialState"">The initial State.</param>
                /// <param name = ""recipe"">The recipe function.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>The next immutable state.</returns>
                [ExcludeFromCodeCoverage]
                public static {interfaceName} Produce({className} initialState, Func<{className}> recipe, ICloneProvider cloneProvider = null) =>
                    Producer(recipe, cloneProvider)(initialState);
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the producer function that accepts a function recipe.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerAction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;

            var method = $@"
                /// <summary>
                /// Creates a Producer delegate that can be used to curry on an Immutable State.
                /// </summary>
                /// <param name = ""recipe"">The recipe action that operates on an object of type T.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>A producer delegate.</returns>
                [ExcludeFromCodeCoverage]
                public static Func<{className}, {interfaceName}> Producer(Action<{className}> recipe, ICloneProvider cloneProvider = null) =>
                    (immutable1) => Producer<object>((argument1, _) => recipe(argument1), cloneProvider)(immutable1, null);
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the producer function that accepts an action recipe.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerFunction(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;

            var method = $@"
                /// <summary>
                /// Creates a Producer delegate that can be used to curry on an Immutable State.
                /// </summary>
                /// <param name = ""recipe"">The recipe function that returns an object of type T.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>A recipe delegate.</returns>
                [ExcludeFromCodeCoverage]
                public static Func<{className}, {interfaceName}> Producer(Func<{className}> recipe, ICloneProvider cloneProvider = null) =>
                    (immutable1) => Producer<object>(_ => recipe(), cloneProvider)(immutable1, null);
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generate the producer which accepts an action recipe with a single Argument.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerActionWithArgument(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;

            var method = $@"
                /// <summary>
                /// Creates a Producer delegate that can be used to curry on an Immutable State.
                /// </summary>
                /// <param name = ""recipe"">The recipe action that operates on an object of type T with a single argument.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <typeparam name = ""TArg"">The type of the argument.</typeparam>
                /// <returns>A producer delegate.</returns>
                [ExcludeFromCodeCoverage]
                public static Func<{className}, TArg, {interfaceName}> Producer<TArg>(Action<{className}, TArg> recipe, ICloneProvider cloneProvider = null) => 
                    (immutable1, argument1) =>
                    {{
                        using var scope = immutable1.CreateDraft<{className}>(out var draft, cloneProvider);
                        recipe(draft, argument1);
                        return scope.FinishDraft<{className},{interfaceName}>(draft);
                    }};
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generate the producer which accepts a function recipe with a single Argument.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerFunctionWithArgument(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;

            var method = $@"
                /// <summary>
                /// Creates a Producer delegate that can be used to curry on an Immutable State.
                /// </summary>
                /// <param name = ""recipe"">The recipe function that returns an object of type T with a single argument.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <typeparam name = ""TArg"">The type of the argument.</typeparam>
                /// <returns>A producer delegate.</returns>
                [ExcludeFromCodeCoverage]
                public static Func<{className}, TArg, {interfaceName}> Producer<TArg>(Func<TArg, {className}> recipe, ICloneProvider cloneProvider = null) =>
                    (immutable1, argument1) =>
                    {{
                        using var scope = immutable1.CreateDraft<{className}>(out var _, cloneProvider);
                        {className} draft = recipe(argument1);
                        return scope.FinishDraft<{className},{interfaceName}>(draft);
                    }};
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }
    }
}
