﻿// <copyright file="DraftScope.cs" company="Jan-Willem Spuij">
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
    using System.Runtime.InteropServices.ComTypes;
    using System.Xml.Schema;
    using static DraftExtensions;

    /// <summary>
    /// Represents a draft scope (A scope in which drafts are created).
    /// </summary>
    public class DraftScope : IDraftScope
    {
        /// <summary>
        /// The producer options.
        /// </summary>
        private readonly IProducerOptions producerOptions;

        /// <summary>
        /// The list of drafts that this scope maintains.
        /// </summary>
        private readonly List<IDraft> drafts = new List<IDraft>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DraftScope"/> class.
        /// </summary>
        /// <param name = "producerOptions">The producer options to use. If you leave them null, the default options will be used.</param>
        internal DraftScope(IProducerOptions producerOptions)
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
            foreach (var iDraft in this.drafts)
            {
                iDraft.DraftState?.Revoke();

                if (iDraft is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            this.drafts.Clear();
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
        public object CreateProxy(object source)
        {
            if (InternalIsDraft(source))
            {
                throw new DraftException(source, "The object is already a draft.");
            }

            Type? proxyType = GetProxyType(source);

            if (proxyType == null)
            {
                throw new DraftException(source, "The object is not draftable.");
            }

            var draftState = (source is System.Collections.IEnumerable) ?
                new CollectionDraftState(this, source) :
                (DraftState)new ObjectDraftState(this, source);

            // TODO: pluggable object creation.
            object result = Activator.CreateInstance(proxyType);
            if (result is IDraft draft)
            {
                this.drafts.Add(draft);
                draft.DraftState = draftState;
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
            object? Reconcile(object? draft)
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

                var proxyType = GetProxyType(draft);

                if (proxyType == null)
                {
                    throw new DraftException(draft, $"The object of type {draftType} cannot be made immutable.");
                }

                if (draft is IDraft idraft)
                {
                    if (idraft.DraftState is ObjectDraftState objectDraftState)
                    {
                        foreach ((string propertyName, object child) in objectDraftState.ChildDrafts)
                        {
                            var immutable = Reconcile(child);
                            if (ReferenceEquals(immutable, child))
                            {
                                // use reflection to set the property and trigger changed on the parent.
                                draftType.GetProperty(propertyName).SetValue(draft, immutable);
                            }
                        }
                    }

                    if (idraft.DraftState!.Changed)
                    {
                        idraft.DraftState = null;
                    }
                    else
                    {
                        draft = idraft.DraftState.GetOriginal<object?>();
                    }
                }

                return draft;
            }

            this.IsFinishing = true;
            try
            {
                draft = Reconcile(draft);

                if (draft is ILockable lockable)
                {
                    lockable.Lock();
                }

                return draft;
            }
            finally
            {
                this.IsFinishing = false;
                this.Dispose();
            }
        }
    }
}
