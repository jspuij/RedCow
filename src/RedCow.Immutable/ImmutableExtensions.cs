// <copyright file="ImmutableExtensions.cs" company="Jan-Willem Spuij">
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
    using RedCow.Immutable;

    /// <summary>
    /// Extension methods for <see cref="Immutable{T}"/>.
    /// </summary>
    public static class ImmutableExtensions
    {
        /// <summary>
        /// Creates a new Draft, based on the <see cref="Immutable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the state.</typeparam>
        /// <param name="state">The immutable.</param>
        /// <param name="draft">A new draft, based on the immutable.</param>
        /// <param name="cloneProvider">The clone provider.</param>
        /// <returns>A scope that is used to either reconcile or dispose of the draft.</returns>
        public static IDraftScope CreateDraft<T>(this T state, out T draft, ICloneProvider? cloneProvider = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new Draft, based on the <see cref="Immutable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the state.</typeparam>
        /// <param name="state">The immutable.</param>
        /// <param name="draft">A new draft, based on the immutable.</param>
        /// <param name="cloneProvider">The clone provider.</param>
        /// <returns>A scope that is used to either reconcile or dispose of the draft.</returns>
        public static IDraftScope CreateDraft<T>(this Immutable<T> state, out T draft, ICloneProvider? cloneProvider = null)
        {
            throw new NotImplementedException();
        }
    }
}
