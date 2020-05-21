// <copyright file="Middleware{TInterface}.cs" company="Jan-Willem Spuij">
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
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// Abstract base class for Middleware.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface that the middleware implements.</typeparam>
    public abstract class Middleware<TInterface>
        where TInterface : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Middleware{TInterface}"/> class.
        /// </summary>
        /// <param name="other">The other middleware to chain to.</param>
        public Middleware(TInterface? other)
        {
            this.Next = other;
        }

        /// <summary>
        /// Gets the next instance of middleware in the chain.
        /// </summary>
        protected TInterface? Next { get; }
    }
}
