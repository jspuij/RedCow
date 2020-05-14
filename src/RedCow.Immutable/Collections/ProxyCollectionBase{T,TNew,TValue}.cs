// <copyright file="ProxyCollectionBase{T,TNew,TValue}.cs" company="Jan-Willem Spuij">
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

namespace RedCow.Immutable.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using RedCow.Immutable;

    /// <summary>
    /// Base class for collection types.
    /// </summary>
    /// <typeparam name="T">The collection type.</typeparam>
    /// <typeparam name="TNew">The concreate implementation in case we have to create a new one.</typeparam>
    /// <typeparam name="TValue">The inner value type.</typeparam>
    public abstract class ProxyCollectionBase<T, TNew, TValue> : ILockable, IDraft, IEnumerable, ICollection
        where T : class, ICollection<TValue>, IEnumerable
        where TNew : T, new()
    {
        /// <summary>
        /// The inner collection.
        /// </summary>
        private T? innerCollection;

        /// <summary>
        /// Gets a value indicating whether the collection is read only.
        /// </summary>
        public bool IsReadOnly => this.Locked || this.CollectionDraftState == null || this.CollectionDraftState.Revoked;

        /// <summary>
        /// Gets a value indicating whether the collection has a fixed size.
        /// </summary>
        public bool IsFixedSize
        {
            get
            {
                this.ThrowIfRevoked();
                return this.IsReadOnly;
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the collection is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized => this.IsReadOnly;

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                this.ThrowIfRevoked();
                return ((ICollection)this.InnerCollection).Count;
            }
        }

        /// <summary>
        /// Gets the original collection if available.
        /// </summary>
        /// <remarks>May return null when this proxy is not in draft state.</remarks>
        public T? Original
        {
            get
            {
                this.ThrowIfRevoked();
                return this.CollectionDraftState?.GetOriginal<T>();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DraftState"/> for this draft.
        /// </summary>
        DraftState? IDraft.DraftState
        {
            get => this.CollectionDraftState;
            set
            {
                if (this.CollectionDraftState != null && value != null)
                {
                    throw new DraftException(this, "Draft state already set.");
                }

                if (this.Locked)
                {
                    throw new ImmutableException(this, "This instance is immutable and cannot be assigned a new Draft state.");
                }

                this.CollectionDraftState = value as CollectionDraftState;

                if (this.CollectionDraftState != null)
                {
                    this.InnerCollection = this.Original!;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the immutable is locked.
        /// </summary>
        bool ILockable.Locked => this.Locked;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        object ICollection.SyncRoot { get; } = new object();

        /// <summary>
        /// Gets or sets the draft state for this collection.
        /// </summary>
        protected CollectionDraftState? CollectionDraftState
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the inner collection and creates one if it does not exist.
        /// </summary>
        protected T InnerCollection
        {
            get
            {
                if (this.innerCollection == null)
                {
                    this.innerCollection = (T)new TNew();
                }

                return this.innerCollection;
            }

            set
            {
                this.innerCollection = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this collection is locked.
        /// </summary>
        protected bool Locked
        {
            get; private set;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TValue> GetEnumerator()
        {
            this.ThrowIfRevoked();
            var enumerator = this.InnerCollection.GetEnumerator();

            if (this.Locked || this.CollectionDraftState == null)
            {
                return enumerator;
            }

            return this.GetDraftEnumerator(enumerator);
        }

        /// <summary>
        /// Locks the Immutable.
        /// </summary>
        void ILockable.Lock()
        {
            this.ThrowIfRevoked();
            if (!this.Locked)
            {
                this.Locked = true;
                var enumerator = this.GetInnerValueEnumerator();

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is ILockable lockable)
                    {
                        lockable.Lock();
                    }
                }
            }
        }

        /// <summary>
        /// Clones the draft from the original.
        /// </summary>
        void IDraft.Clone()
        {
            if (this.CollectionDraftState == null)
            {
                throw new DraftException(this, "Draft state not set.");
            }

            this.ThrowIfReadonly();

            this.CopyOnWrite();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            this.ThrowIfRevoked();
            return this.GetEnumerator();
        }

        /// <summary>
        /// Copies the elements of the collection to an System.Array, starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional System.Array that is the destination of the elements copied
        /// from System.Collections.ICollection. The System.Array must have zero-based indexing.
        /// </param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        void ICollection.CopyTo(Array array, int index)
        {
            this.ThrowIfRevoked();
            ((ICollection)this.InnerCollection).CopyTo(array, index);
        }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The object to add to the collection.</param>
        public void Add(TValue item)
        {
            this.ThrowIfReadonly();
            this.CollectionDraftState!.Modify(() => this.InnerCollection.Add(item), this.CopyOnWrite);
        }

        /// <summary>
        ///  Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            this.ThrowIfReadonly();
            this.CollectionDraftState!.Modify(() => this.InnerCollection.Clear(), this.CopyOnWrite);
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>true if item is found in the collection; otherwise, false.</returns>
        public bool Contains(TValue item)
        {
            this.ThrowIfRevoked();
            return this.InnerCollection.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the collection. to an System.Array,
        /// starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The array index.</param>
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            this.ThrowIfRevoked();
            this.InnerCollection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The object to remove from the collection.</param>
        /// <returns>
        /// true if item was successfully removed from the collection;
        /// otherwise, false. This method also returns false if item is not found in the
        /// original collection.</returns>
        public bool Remove(TValue item)
        {
            this.ThrowIfReadonly();
            bool result = false;
            this.CollectionDraftState!.Modify(() => result = this.InnerCollection.Remove(item), this.CopyOnWrite);
            return result;
        }

        /// <summary>
        /// Throws an exception that this collection is readonly.
        /// </summary>
        protected void ThrowIfReadonly()
        {
            this.ThrowIfRevoked();
            if (this.IsReadOnly)
            {
                throw new ImmutableException(this, $"This {this.GetType()} is immutable and cannot be modified.");
            }
        }

        /// <summary>
        /// Throws an exception that this collection is revoked.
        /// </summary>
        protected void ThrowIfRevoked()
        {
            if (this.CollectionDraftState?.Revoked ?? false)
            {
                throw new DraftRevokedException(this, $"This {this.GetType()} instance is revoked.");
            }
        }

        /// <summary>
        /// Gets the value enumerator.
        /// </summary>
        /// <returns>An enumerator with values.</returns>
        protected abstract System.Collections.IEnumerator GetInnerValueEnumerator();

        /// <summary>
        /// Gets an enumerator that creates drafts while enumerating.
        /// </summary>
        /// <param name="enumerator">The source enumerator.</param>
        /// <returns>An enumerator that creates drafts.</returns>
        protected abstract IEnumerator<TValue> GetDraftEnumerator(IEnumerator<TValue> enumerator);

        /// <summary>
        /// Executes the Copy on write on the inner list.
        /// </summary>
        protected abstract void CopyOnWrite();
    }
}
