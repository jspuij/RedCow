// <copyright file="ProducerOptions.cs" company="Jan-Willem Spuij">
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
    using RedCow.Immutable;

    /// <summary>
    /// The producer options to use.
    /// </summary>
    public class ProducerOptions : IProducerOptions
    {
        /// <summary>
        /// Gets the default Producer options.
        /// </summary>
        public static IProducerOptions Default { get; } = new ProducerOptions();

        /// <summary>
        /// Gets or sets the Clone Provider.
        /// </summary>
        public ICloneProvider CloneProvider { get; set; } = new ReflectionCloneProvider();

        /// <summary>
        /// Gets the set of allowed immutable reference types.
        /// </summary>
        public ISet<Type> AllowedImmutableReferenceTypes { get; } = new HashSet<Type>()
        {
            typeof(string),
            typeof(Type),
        };

        /// <summary>
        /// Gets or sets maximum recursion depth during Reconciliation.
        /// </summary>
        public int MaxDepth { get; set; } = 1000;
    }
}
