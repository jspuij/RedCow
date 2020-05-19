// <copyright file="CollectionDraftState.cs" company="Jan-Willem Spuij">
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
    using RedCow.Immutable.Patches;
    using static DraftExtensions;

    /// <summary>
    /// A draft state for Collections.
    /// </summary>
    public class CollectionDraftState : DraftState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionDraftState"/> class.
        /// </summary>
        /// <param name="scope">The scope this draft state belongs to.</param>
        /// <param name="original">The original.</param>
        /// <param name="path">The path segment.</param>
        public CollectionDraftState(DraftScope scope, object original, PathSegment? path)
            : base(scope, original, path)
        {
        }

        /// <summary>
        /// Gets a proxy based on the get and set functions provided.
        /// </summary>
        /// <typeparam name="T">The proxy type.</typeparam>
        /// <param name="index">The index path segment.</param>
        /// <param name="getter">The getter.</param>
        /// <param name="setter">The setter.</param>
        /// <param name="copyOnWrite">The copy on write method.</param>
        /// <returns>The proxied value.</returns>
        public T Get<T>(string index, Func<T> getter, Action<T> setter, Action copyOnWrite)
        {
            if (this.Revoked)
            {
                throw new DraftRevokedException(this, "Exception while getting element: The draft is out of scope and has been revoked.");
            }

            var result = getter();

            if (result == null)
            {
                return result;
            }

            var resultType = result.GetType();

            if (resultType.IsValueType || this.Scope.AllowedImmutableReferenceTypes.Contains(resultType))
            {
                return result;
            }

            if (InternalGetDraftState(result)?.Scope == this.Scope || this.Scope.IsFinishing)
            {
                return result;
            }

            PathSegment? path = (this.Path != null) ? new PathSegment(this.Path, index) : null;

            result = (T)this.Scope.CreateProxy(result, path);
            copyOnWrite();
            setter(result);

            return result;
        }

        /// <summary>
        /// Modifies the attached collection using the specified modify action,
        /// optionally first executing the copy on write action.
        /// </summary>
        /// <param name="modify">The modification to the collection.</param>
        /// <param name="copyOnWrite">The copy on write action.</param>
        public void Modify(Action modify, Action copyOnWrite)
        {
            if (this.Revoked)
            {
                throw new DraftRevokedException(this, "Exception while modifying element: The draft is out of scope and has been revoked.");
            }

            if (!this.Scope.IsFinishing && !this.Changed)
            {
                this.Changed = true;
                copyOnWrite();
            }

            modify();
        }
    }
}
