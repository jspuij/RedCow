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
                this.GenerateProducerAction(interfaceDeclaration, false),
                this.GenerateProducerFunction(interfaceDeclaration, false),
                this.GenerateProducerActionWithArgument(interfaceDeclaration, false),
                this.GenerateProducerFunctionWithArgument(interfaceDeclaration, false),
                this.GenerateProducerAction(interfaceDeclaration, true),
                this.GenerateProducerFunction(interfaceDeclaration, true),
                this.GenerateProducerActionWithArgument(interfaceDeclaration, true),
                this.GenerateProducerFunctionWithArgument(interfaceDeclaration, true));
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
                ///intial state.
                /// </summary>
                /// <param name = ""initialState"">The initial State.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>The next immutable state.</returns>
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
                /// specified producer action.
                /// </summary>
                /// <param name = ""producer"">The producer action.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>The next immutable state.</returns>
                    public {interfaceName} Produce(Action<{className}> producer, ICloneProvider cloneProvider = null) => Produce(({className})this, producer, cloneProvider);
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
                /// specified producer action.
                /// </summary>
                /// <param name = ""initialState"">The initial State.</param>
                /// <param name = ""producer"">The producer action.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>The next immutable state.</returns>
                    public static {interfaceName} Produce({className} initialState, Action<{className}> producer, ICloneProvider cloneProvider = null) =>
                        InitialProducer(producer, cloneProvider)(initialState);
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
                /// specified producer function.
                /// </summary>
                /// <param name = ""producer"">The producer function.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>The next immutable state.</returns>
                public {interfaceName} Produce(Func<{className}> producer, ICloneProvider cloneProvider = null) => Produce(({className})this, producer, cloneProvider);
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
                /// specified producer function.
                /// </summary>
                /// <param name = ""initialState"">The initial State.</param>
                /// <param name = ""producer"">The producer function.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>The next immutable state.</returns>
                public static {interfaceName} Produce({className} initialState, Func<{className}> producer, ICloneProvider cloneProvider = null) =>
                    InitialProducer(producer, cloneProvider)(initialState);
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the producer function that accepts a function.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <param name="useInterface">Whether to use the interface declaration for the initialstate.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerAction(InterfaceDeclarationSyntax interfaceDeclaration, bool useInterface)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;
            var initialStateTypeName = useInterface ? interfaceName : className;
            var producerName = useInterface ? "Producer" : "InitialProducer";

            var method = $@"
                /// <summary>
                /// Creates a Producer delegate that can be used to curry on an Immutable State.
                /// </summary>
                /// <param name = ""producer"">The producer action that operates on an object of type T.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>A producer delegate.</returns>
                public static Func<{initialStateTypeName}, {interfaceName}> {producerName}(Action<{className}> producer, ICloneProvider cloneProvider = null) =>
                    (immutable1) => {producerName}<object>((argument1, _) => producer(argument1), cloneProvider)(immutable1, null);
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generates the producer function that accepts an action.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <param name="useInterface">Whether to use the interface declaration for the initialstate.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerFunction(InterfaceDeclarationSyntax interfaceDeclaration, bool useInterface)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;
            var initialStateTypeName = useInterface ? interfaceName : className;
            var producerName = useInterface ? "Producer" : "InitialProducer";

            var method = $@"
                /// <summary>
                /// Creates a Producer delegate that can be used to curry on an Immutable State.
                /// </summary>
                /// <param name = ""producer"">The producer function that returns an object of type T.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <returns>A producer delegate.</returns>
                public static Func<{initialStateTypeName}, {interfaceName}> {producerName}(Func<{className}> producer, ICloneProvider cloneProvider = null) =>
                    (immutable1) => {producerName}<object>(_ => producer(), cloneProvider)(immutable1, null);
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generate the producer which accepts an action with a single Argument.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <param name="useInterface">Whether to use the interface declaration for the initialstate.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerActionWithArgument(InterfaceDeclarationSyntax interfaceDeclaration, bool useInterface)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;
            var initialStateTypeName = useInterface ? interfaceName : className;
            var producerName = useInterface ? "Producer" : "InitialProducer";

            var method = $@"
                /// <summary>
                /// Creates a Producer delegate that can be used to curry on an Immutable State.
                /// </summary>
                /// <param name = ""producer"">The producer action that operates on an object of type T with a single argument.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <typeparam name = ""TArg"">The type of the argument.</typeparam>
                /// <returns>A producer delegate.</returns>
                    public static Func<{initialStateTypeName}, TArg, {interfaceName}> {producerName}<TArg>(Action<{className}, TArg> producer, ICloneProvider cloneProvider = null) => 
                        (immutable1, argument1) =>
                        {{
                            using var scope = immutable1.CreateDraft<{className}>(out var draft, cloneProvider);
                            producer(draft, argument1);
                            return scope.FinishDraft<{className},{interfaceName}>(draft);
                        }};
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }

        /// <summary>
        /// Generate the producer which accepts a function with a single Argument.
        /// </summary>
        /// <param name="interfaceDeclaration">The interface declaration.</param>
        /// <param name="useInterface">Whether to use the interface declaration for the initialstate.</param>
        /// <returns>The method declaration.</returns>
        private MemberDeclarationSyntax GenerateProducerFunctionWithArgument(InterfaceDeclarationSyntax interfaceDeclaration, bool useInterface)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var className = this.mutableType.Name;
            var initialStateTypeName = useInterface ? interfaceName : className;
            var producerName = useInterface ? "Producer" : "InitialProducer";

            var method = $@"
                /// <summary>
                /// Creates a Producer delegate that can be used to curry on an Immutable State.
                /// </summary>
                /// <param name = ""producer"">The producer function that returns an object of type T with a single argument.</param>
                /// <param name = ""cloneProvider"">The clone provider to use.</param>
                /// <typeparam name = ""TArg"">The type of the argument.</typeparam>
                /// <returns>A producer delegate.</returns>
                    public static Func<{initialStateTypeName}, TArg, {interfaceName}> {producerName}<TArg>(Func<TArg, {className}> producer, ICloneProvider cloneProvider = null) =>
                        (immutable1, argument1) =>
                        {{
                            using var scope = immutable1.CreateDraft<{className}>(out var _, cloneProvider);
                            {className} draft = producer(argument1);
                            return scope.FinishDraft<{className},{interfaceName}>(draft);
                        }};
            ";
            return ParseMemberDeclaration(method)
            .NormalizeWhitespace();
        }
    }
}
