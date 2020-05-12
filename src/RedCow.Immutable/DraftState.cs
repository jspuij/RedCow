﻿// <copyright file="DraftState.cs" company="Jan-Willem Spuij">
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
    using static DraftExtensions;

    /// <summary>
    /// State for drafts.
    /// </summary>
    public class DraftState
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
        /// Gets a value indicating whether or not this draft is revoked.
        /// </summary>
        public bool Revoked { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not this draft is changed.
        /// </summary>
        public bool Changed { get; internal set; }

        /// <summary>
        /// Gets the scope this draft belongs to.
        /// </summary>
        public DraftScope Scope { get; }

        /// <summary>
        /// Gets child draft proxies.
        /// </summary>
        public IDictionary<string, object> Children { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the original.
        /// </summary>
        /// <typeparam name="T">The type to cast the original to.</typeparam>
        /// <returns>The original.</returns>
        public T GetOriginal<T>() => (T)this.original;

        /// <summary>
        /// Gets a property value, possibly drafting it while getting it.
        /// </summary>
        /// <typeparam name="T">The type of the Property.</typeparam>
        /// <param name="propertyName">The property name.</param>
        /// <param name="getter">The getter.</param>
        /// <param name="originalGetter">The getter to get the original property.</param>
        /// <param name="setter">The setter.</param>
        /// <exception cref="InvalidOperationException">thrown when the draft is revoked.</exception>
        /// <returns>The property value.</returns>
        public T Get<T>(string propertyName, Func<T> getter, Func<T> originalGetter, Action<T> setter)
        {
            if (this.Revoked)
            {
                throw new DraftRevokedException(this, $"Exception while getting property {propertyName}: The draft is out of scope and has been revoked.");
            }

            var result = this.Changed ? getter() : originalGetter();

            if (result == null)
            {
                return result;
            }

            var resultType = result.GetType();

            if (resultType.IsValueType || this.Scope.AllowedImmutableReferenceTypes.Contains(resultType))
            {
                return result;
            }

            if (!this.Changed)
            {
                var proxy = getter();
                if (proxy != null)
                {
                    result = proxy;
                }
            }

            if (InternalIsDraft(result) || this.Scope.IsFinishing)
            {
                return result;
            }

            result = (T)this.Scope.CreateProxy(result);
            this.Children.Add(propertyName, result);
            setter(result);

            return result;
        }

        /// <summary>
        /// Sets a property value.
        /// </summary>
        /// <typeparam name="T">The type of the Property.</typeparam>
        /// <param name="propertyName">The property name.</param>
        /// <param name="setter">The setter.</param>
        /// <param name="copyOnWrite">The copy on write action.</param>
        /// <exception cref="InvalidOperationException">thrown when the draft is revoked.</exception>
        public void Set<T>(string propertyName, Action setter, Action copyOnWrite)
        {
            if (this.Revoked)
            {
                throw new DraftRevokedException(this, $"Exception while setting property {propertyName}: The draft is out of scope and has been revoked.");
            }

            if (!this.Changed)
            {
                this.Changed = true;
                copyOnWrite();
            }

            setter();
        }

        /// <summary>
        /// Revokes the draft.
        /// </summary>
        internal void Revoke()
        {
            this.Revoked = true;
        }
    }
}
