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
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Represents a draft scope (A scope in which drafts are created).
    /// </summary>
    public class DraftScope : IDraftScope
    {
        /// <summary>
        /// A list of drafts.
        /// </summary>
        private readonly List<IDraft> drafts = new List<IDraft>();

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
        /// Gets or sets the parent <see cref="DraftScope"/>.
        /// </summary>
        public DraftScope? Parent { get; set; }

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
            if (draft == null)
            {
                throw new ArgumentNullException(nameof(draft));
            }

            Type? type = typeof(T);

            Type? immutableType = null;

            while (type != null)
            {
                if (type.GetCustomAttributes().SingleOrDefault(x => x is ImmutableTypeAttribute) is ImmutableTypeAttribute immutableTypeAttribute)
                {
                    immutableType = immutableTypeAttribute.ImmutableType;
                    break;
                }

                type = type.BaseType;
            }

            if (immutableType == null)
            {
                throw new InvalidOperationException();
            }

            // TODO: Implement finish draft.
            var result = (TInterface)Activator.CreateInstance(immutableType);
            this.CloneProvider.Clone(draft, result);
            return result;
        }

        /// <summary>
        /// Creates a draft proxy using the current scope and clone provider.
        /// </summary>
        /// <param name="source">The source object to create the proxy for.</param>
        /// <exception cref="InvalidOperationException">When the source object is not draftable.</exception>
        /// <returns>An instance of type T.</returns>
        internal object CreateProxy(object source)
        {
            Type? type = source.GetType();

            Type? draftType = null;

            while (type != null)
            {
                if (type.GetCustomAttributes().SingleOrDefault(x => x is DraftTypeAttribute) is DraftTypeAttribute draftTypeAttribute)
                {
                    draftType = draftTypeAttribute.DraftType;
                    break;
                }

                type = type.BaseType;
            }

            if (draftType == null)
            {
                throw new InvalidOperationException();
            }

            var draftState = new DraftState(this, source);

            // TODO: pluggable object creation.
            object result = Activator.CreateInstance(draftType, draftState);
            this.CloneProvider.Clone(source, result);
            this.drafts.Add((IDraft)result);
            return result;
        }
    }
}
