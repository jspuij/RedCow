// <copyright file="DraftState.cs" company="Jan-Willem Spuij">
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

namespace RedCow.Immutable
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// State for drafts.
    /// </summary>
    public abstract class DraftState
    {
        /// <summary>
        /// The original object.
        /// </summary>
        private readonly object original;

        /// <summary>
        /// Initializes a new instance of the <see cref="DraftState"/> class.
        /// </summary>
        /// <param name="scope">The scope this draft state belongs to.</param>
        /// <param name="original">The original.</param>
        public DraftState(DraftScope scope, object original)
        {
            this.Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            this.original = original ?? throw new ArgumentNullException(nameof(original));
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this draft is revoked.
        /// </summary>
        public bool Revoked { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether or not this draft is changed.
        /// </summary>
        public bool Changed { get; internal set; }

        /// <summary>
        /// Gets the scope this draft belongs to.
        /// </summary>
        public DraftScope Scope { get; private set;  }

        /// <summary>
        /// Gets the original.
        /// </summary>
        /// <typeparam name="T">The type to cast the original to.</typeparam>
        /// <returns>The original.</returns>
        public T GetOriginal<T>() => (T)this.original;

        /// <summary>
        /// Revokes the draft.
        /// </summary>
        public void Revoke()
        {
            this.Revoked = true;
        }
    }
}
