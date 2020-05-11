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
    using static DraftExtensions;

    /// <summary>
    /// Represents a draft scope (A scope in which drafts are created).
    /// </summary>
    public class DraftScope : IDraftScope, IProducerOptions
    {
        /// <summary>
        /// A list of drafts.
        /// </summary>
        private readonly List<IDraft> drafts = new List<IDraft>();

        /// <summary>
        /// The producer options.
        /// </summary>
        private readonly IProducerOptions producerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DraftScope"/> class.
        /// </summary>
        /// <param name = "producerOptions">The producer options to use. If you leave them null, the default options will be used.</param>
        public DraftScope(IProducerOptions producerOptions)
        {
            this.producerOptions = producerOptions ?? throw new ArgumentNullException(nameof(producerOptions));
        }

        /// <summary>
        /// Gets the Clone Provider for this scope.
        /// </summary>
        public ICloneProvider CloneProvider => this.producerOptions.CloneProvider;

        /// <summary>
        /// Gets or sets the parent <see cref="DraftScope"/>.
        /// </summary>
        public DraftScope? Parent { get; set; }

        /// <summary>
        /// Gets the allowed immutable reference types.
        /// </summary>
        public ISet<Type> AllowedImmutableReferenceTypes =>
            this.producerOptions.AllowedImmutableReferenceTypes;

        /// <summary>
        /// Gets a value indicating whether this scope is finishing.
        /// </summary>
        internal bool IsFinishing { get; private set; }

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

            return (TInterface)this.FinishInstance(draft) !;
        }

        /// <summary>
        /// Creates a draft proxy using the current scope and clone provider.
        /// </summary>
        /// <param name="source">The source object to create the proxy for.</param>
        /// <exception cref="InvalidOperationException">When the source object is not draftable.</exception>
        /// <returns>An instance of type T.</returns>
        internal object CreateProxy(object source)
        {
            if (InternalIsDraft(source))
            {
                throw new InvalidOperationException("The object is already a draft.");
            }

            Type? draftType = GetDraftType(source);

            var draftState = new DraftState(this, source);

            if (draftType == null)
            {
                throw new InvalidOperationException("The object is not draftable.");
            }

            // TODO: pluggable object creation.
            object result = this.CloneProvider.Clone(source, () => Activator.CreateInstance(draftType, draftState), source => source);

            if (result is IDraft draft)
            {
                draft.DraftState.StartTracking();
                this.drafts.Add(draft);
            }

            return result;
        }

        /// <summary>
        /// Finishes an instance.
        /// </summary>
        /// <param name="draft">The instance to finish.</param>
        /// <returns>The immutable variant of the instance.</returns>
        private object? FinishInstance(object? draft)
        {
            object? FinishInstanceInternal(object? draft)
            {
                if (draft == null)
                {
                    return null;
                }

                var draftType = draft.GetType();

                if (draftType.IsValueType || this.AllowedImmutableReferenceTypes.Contains(draftType))
                {
                    return draft;
                }

                var immutableType = GetImmutableType(draft);

                if (immutableType == null)
                {
                    throw new InvalidOperationException($"The object of type {draftType} cannot be made immutable.");
                }

                if (draft.GetType() == immutableType)
                {
                    return draft;
                }

                // return the original if the draft did not change. Saves another copy.
                if (draft is IDraft idraft && !idraft.DraftState.Changed)
                {
                    var original = idraft.DraftState.GetOriginal<object?>();

                    if (original is ILockable)
                    {
                        return original;
                    }
                }

                // TODO: pluggable object creation.
                object? result = this.CloneProvider.Clone(draft, () => Activator.CreateInstance(immutableType), FinishInstanceInternal);

                // lock the immutable.
                if (result is ILockable lockable && !lockable.Locked)
                {
                    lockable.Lock();
                }

                // revoke the draft.
                if (draft is IDraft toRevoke)
                {
                    toRevoke.DraftState.Revoke();
                }

                return result;
            }

            this.IsFinishing = true;
            try
            {
                return FinishInstanceInternal(draft);
            }
            finally
            {
                this.IsFinishing = false;
            }
        }
    }
}
