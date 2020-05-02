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
        /// Initializes a new instance of the <see cref="ProducerInterfaceGenerator"/> class.
        /// </summary>
        /// <param name="attributeData">The attribute data to use.</param>
        public ProducerInterfaceGenerator(AttributeData attributeData)
        {
            // System.Diagnostics.Debugger.Launch();
            // while (!System.Diagnostics.Debugger.IsAttached)
            // {
            //    Thread.Sleep(500); // eww, eww, eww
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
        /// <param name="applyToInterface">The interface to apply the partial to.</param>
        /// <returns>The interface declaration syntax.</returns>
        private InterfaceDeclarationSyntax GeneratePartial(InterfaceDeclarationSyntax applyToInterface)
        {
            throw new NotImplementedException();
        }
    }
}
