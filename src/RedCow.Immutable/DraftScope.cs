// <copyright file="DraftScope.cs" company="Jan-Willem Spuij">
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
    using System.Text;

    /// <summary>
    /// Represents a draft scope (A scope in which drafts are created).
    /// </summary>
    public class DraftScope : IDraftScope
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DraftScope"/> class.
        /// </summary>
        /// <param name="cloneProvider">The clone provider to use for drafts inside the scope.</param>
        public DraftScope(ICloneProvider cloneProvider)
        {
            this.CloneProvider = cloneProvider ?? throw new ArgumentNullException(nameof(cloneProvider));
        }

        /// <summary>
        /// Gets the Clone Provider for this scope.
        /// </summary>
        public ICloneProvider CloneProvider { get; }

        /// <summary>
        /// Cleans up the scope.
        /// </summary>
        public void Dispose()
        {
            // TODO: Implement sane disposal.
        }

        /// <summary>
        /// Finishes a draft and returns the next state.
        /// </summary>
        /// <typeparam name="T">The type of the draft.</typeparam>
        /// <typeparam name="TInterface">The immutable type.</typeparam>
        /// <param name="draft">The draft.</param>
        /// <returns>The immutable.</returns>
        public TInterface FinishDraft<T, TInterface>(T draft)
        {
            // TODO: Implement finish draft.
            throw new NotImplementedException();
        }
    }
}
