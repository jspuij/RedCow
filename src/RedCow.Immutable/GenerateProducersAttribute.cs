// <copyright file="GenerateProducersAttribute.cs" company="Jan-Willem Spuij">
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

namespace RedCow
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using CodeGeneration.Roslyn;

    /// <summary>
    /// Attribute to indicate that producer methods should be generated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    [CodeGenerationAttribute("RedCow.Generators.ProducerInterfaceGenerator, RedCow.Generators")]
    [Conditional("CodeGeneration")]
    [ExcludeFromCodeCoverage]
    public class GenerateProducersAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateProducersAttribute"/> class.
        /// </summary>
        /// <param name="mutableType">
        /// The type of the Immutable interface that needs to be implemented.
        /// </param>
        public GenerateProducersAttribute(Type mutableType)
        {
            this.MutableType = mutableType;
        }

        /// <summary>
        /// Gets the type of the Immutable Interface.
        /// </summary>
        public Type MutableType { get; }
    }
}
