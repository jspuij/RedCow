// <copyright file="DraftExtensions.cs" company="Jan-Willem Spuij">
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
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using RedCow.Immutable;

    /// <summary>
    /// Extension methods for drafts.
    /// </summary>
    public static class DraftExtensions
    {
        /// <summary>
        /// Creates a new Draft, based on the type of the state.
        /// </summary>
        /// <typeparam name="T">The type of the state.</typeparam>
        /// <param name="state">The immutable.</param>
        /// <param name="draft">A new draft, based on the immutable.</param>
        /// <param name="cloneProvider">The clone provider.</param>
        /// <returns>A scope that is used to either reconcile or dispose of the draft.</returns>
        public static IDraftScope CreateDraft<T>(this T state, out T draft, ICloneProvider? cloneProvider = null)
            where T : class
        {
            return InternalCreateDraft<T>(state, out draft, cloneProvider);
        }

        /// <summary>
        /// Tests whether an object is a draft.
        /// </summary>
        /// <typeparam name="T">The type to test.</typeparam>
        /// <param name="state">The state to test.</param>
        /// <returns>A value indicating whether the object is a draft.</returns>
        public static bool IsDraft<T>(this T state)
            where T : class
        {
            return InternalIsDraft(state);
        }

        /// <summary>
        /// Tests whether an object is a draftable.
        /// </summary>
        /// <typeparam name="T">The type to test.</typeparam>
        /// <param name="state">The state to test.</param>
        /// <returns>A value indicating whether the object is draftable.</returns>
        public static bool IsDraftable<T>(this T state)
            where T : class
        {
            return InternalIsDraftable(state);
        }

        /// <summary>
        /// Creates a new Draft, based on the type of the state.
        /// </summary>
        /// <typeparam name="T">The type of the state.</typeparam>
        /// <param name="state">The immutable.</param>
        /// <param name="draft">A new draft, based on the immutable.</param>
        /// <param name="cloneProvider">The clone provider.</param>
        /// <returns>A scope that is used to either reconcile or dispose of the draft.</returns>
        private static IDraftScope InternalCreateDraft<T>(object state, out T draft, ICloneProvider? cloneProvider = null)
            where T : class
        {
            // TODO: Add clone provider detection.
            var scope = new DraftScope(cloneProvider ?? new ReflectionCloneProvider());

            try
            {
                if (InternalIsDraft(state))
                {
                    scope.Parent = ((IDraft)state).DraftState.Scope;
                }

                draft = scope.CreateProxy<T>(state);
                return scope;
            }
            catch
            {
                scope.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Tests whether an object is a draft.
        /// </summary>
        /// <param name="state">The state to test.</param>
        /// <returns>A value indicating whether the object is a draft.</returns>
        /// <exception cref="ArgumentNullException">when the state is null.</exception>
        private static bool InternalIsDraft(object state)
        {
            if (state is null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return state is IDraft;
        }

        /// <summary>
        /// Tests whether an object is a draftable.
        /// </summary>
        /// <param name="state">The state to test.</param>
        /// <returns>A value indicating whether the object is draftable.</returns>
        /// <exception cref="ArgumentNullException">when the state is null.</exception>
        private static bool InternalIsDraftable(object state)
        {
            if (state is null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var type = state.GetType();

            while (type != null)
            {
                if (type.GetCustomAttributes().Any(x => x is DraftTypeAttribute))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
