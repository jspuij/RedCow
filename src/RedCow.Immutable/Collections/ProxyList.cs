// <copyright file="ProxyList.cs" company="Jan-Willem Spuij">
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
    using System.Resources;
    using System.Text;
    using RedCow.Immutable;

    /// <summary>
    /// A list that can act as a proxy around another list.
    /// </summary>
    /// <typeparam name="T">The type that is held in the list.</typeparam>
    public class ProxyList<T> : IDraft, ILockable, ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
    {
        /// <summary>
        /// The draft state for this list.
        /// </summary>
        private CollectionDraftState? collectionDraftState;

        /// <summary>
        /// The inner list.
        /// </summary>
        private IList<T>? innerList;

        /// <summary>
        /// A variable indicating whether this list is locked.
        /// </summary>
        private bool locked;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ProxyList{T}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                this.ThrowIfRevoked();
                return this.InnerList.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="ProxyList{T}"/> is read only.
        /// </summary>
        public bool IsReadOnly => this.locked || this.collectionDraftState == null || this.collectionDraftState.Revoked;

        /// <summary>
        /// Gets the original collection if available.
        /// </summary>
        /// <remarks>May return null when this proxy is not in draft state.</remarks>
        public IList<T>? Original
        {
            get
            {
                this.ThrowIfRevoked();
                return this.collectionDraftState?.GetOriginal<IList<T>>();
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="ProxyList{T}" /> is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized => this.IsReadOnly;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ProxyList{T}" />.
        /// </summary>
        public object SyncRoot { get; } = new object();

        /// <summary>
        /// Gets or sets the <see cref="DraftState"/> for this draft.
        /// </summary>
        DraftState? IDraft.DraftState
        {
            get => this.collectionDraftState;
            set
            {
                if (this.collectionDraftState != null && value != null)
                {
                    throw new DraftException(this, "Draft state already set.");
                }

                if (this.locked)
                {
                    throw new ImmutableException(this, "This instance is immutable and cannot be assigned a new Draft state.");
                }

                this.collectionDraftState = value as CollectionDraftState;

                if (this.collectionDraftState != null)
                {
                    this.InnerList = this.Original!;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the immutable is locked.
        /// </summary>
        bool ILockable.Locked => this.locked;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ProxyList{T}"/> has a fixed size.
        /// </summary>
        bool IList.IsFixedSize
        {
            get
            {
                this.ThrowIfRevoked();
                return ((IList)this.InnerList).IsFixedSize;
            }
        }

        /// <summary>
        /// Gets or sets the inner list and creates one if it does not exist.
        /// </summary>
        private IList<T> InnerList
        {
            get
            {
                if (this.innerList == null)
                {
                    this.innerList = new List<T>();
                }

                return this.innerList;
            }

            set
            {
                this.innerList = value;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            get
            {
                this.ThrowIfRevoked();

                if (this.locked || this.collectionDraftState == null)
                {
                    return this.InnerList[index];
                }

                return this.collectionDraftState!.Get(() => this.InnerList[index], value => this.InnerList[index] = value, this.CopyOnWrite);
            }

            set
            {
                this.ThrowIfReadonly();
                this.collectionDraftState!.Modify(() => this.InnerList[index] = value, this.CopyOnWrite);
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        object? IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value!;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            this.ThrowIfRevoked();
            var enumerator = this.InnerList.GetEnumerator();

            if (this.locked || this.collectionDraftState == null)
            {
                return enumerator;
            }

            return this.GetDraftEnumerator(enumerator);
        }

        /// <summary>
        /// Adds an item to the <see cref="ProxyList{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ProxyList{T}"/>.</param>
        public void Add(T item)
        {
            this.ThrowIfReadonly();
            this.collectionDraftState!.Modify(() => this.InnerList.Add(item), this.CopyOnWrite);
        }

        /// <summary>
        ///  Removes all items from the <see cref="ProxyList{T}"/>.
        /// </summary>
        public void Clear()
        {
            this.ThrowIfReadonly();
            this.collectionDraftState!.Modify(() => this.InnerList.Clear(), this.CopyOnWrite);
        }

        /// <summary>
        /// Determines whether the <see cref="ProxyList{T}"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProxyList{T}"/>.</param>
        /// <returns>true if item is found in the <see cref="ProxyList{T}"/>; otherwise, false.</returns>
        public bool Contains(T item)
        {
            this.ThrowIfRevoked();
            return this.InnerList.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ProxyList{T}"/> to an System.Array,
        /// starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The array index.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.ThrowIfRevoked();
            this.InnerList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ProxyList{T}"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ProxyList{T}"/>.</param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="ProxyList{T}"/>;
        /// otherwise, false. This method also returns false if item is not found in the
        /// original <see cref="ProxyList{T}"/>.</returns>
        public bool Remove(T item)
        {
            this.ThrowIfReadonly();
            bool result = false;
            this.collectionDraftState!.Modify(() => result = this.InnerList.Remove(item), this.CopyOnWrite);
            return result;
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="ProxyList{T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProxyList{T}"/>.</param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            this.ThrowIfRevoked();
            return this.InnerList.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="ProxyList{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="ProxyList{T}"/>.</param>
        public void Insert(int index, T item)
        {
            this.ThrowIfReadonly();
            this.collectionDraftState!.Modify(() => this.InnerList.Insert(index, item), this.CopyOnWrite);
        }

        /// <summary>
        /// Removes the <see cref="ProxyList{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            this.ThrowIfReadonly();
            this.collectionDraftState!.Modify(() => this.InnerList.RemoveAt(index), this.CopyOnWrite);
        }

        /// <summary>
        /// Clones the draft from the original.
        /// </summary>
        void IDraft.Clone()
        {
            if (this.collectionDraftState == null)
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
        /// Locks the Immutable.
        /// </summary>
        void ILockable.Lock()
        {
            this.ThrowIfRevoked();
            if (!this.locked)
            {
                this.locked = true;
                foreach (T item in this.InnerList)
                {
                    if (item is ILockable lockable)
                    {
                        lockable.Lock();
                    }
                }
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="ProxyList{T}"/> to an System.Array, starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional System.Array that is the destination of the elements copied
        /// from System.Collections.ICollection. The System.Array must have zero-based indexing.
        /// </param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        void ICollection.CopyTo(Array array, int index)
        {
            this.ThrowIfRevoked();
            ((ICollection)this.InnerList).CopyTo(array, index);
        }

        /// <summary>
        /// Adds an item to the <see cref="ProxyList{T}"/>.
        /// </summary>
        /// <param name="value">The object to add to the <see cref="ProxyList{T}"/>.</param>
        /// <returns>The index of the item that was added.</returns>
        int IList.Add(object value)
        {
            this.ThrowIfReadonly();
            int index = 0;
            this.collectionDraftState!.Modify(() => index = ((IList)this.InnerList).Add(value), this.CopyOnWrite);
            return index;
        }

        /// <summary>
        /// Determines whether the <see cref="ProxyList{T}"/> contains a specific value.
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="ProxyList{T}"/>.</param>
        /// <returns>true if item is found in the <see cref="ProxyList{T}"/>; otherwise, false.</returns>
        bool IList.Contains(object value) => this.Contains((T)value);

        /// <summary>
        /// Determines the index of a specific item in the <see cref="ProxyList{T}"/>.
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="ProxyList{T}"/>.</param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        int IList.IndexOf(object value) => this.IndexOf((T)value);

        /// <summary>
        /// Inserts an item to the <see cref="ProxyList{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="value">The object to insert into the <see cref="ProxyList{T}"/>.</param>
        void IList.Insert(int index, object value) => this.Insert(index, (T)value);

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ProxyList{T}"/>.
        /// </summary>
        /// <param name="value">The object to remove from the <see cref="ProxyList{T}"/>.</param>
        void IList.Remove(object value) => this.Remove((T)value);

        /// <summary>
        /// Gets an enumerator that creates drafts while enumerating.
        /// </summary>
        /// <param name="enumerator">The source enumerator.</param>
        /// <returns>An enumerator that creates drafts.</returns>
        private IEnumerator<T> GetDraftEnumerator(IEnumerator<T> enumerator)
        {
            int index = 0;
            while (enumerator.MoveNext())
            {
                index += 1;
                yield return this.collectionDraftState!.Get(() => enumerator.Current, value => this.InnerList[index] = value, this.CopyOnWrite);
            }
        }

        /// <summary>
        /// Throws an exception <see cref="ProxyList{T}"/> is readonly.
        /// </summary>
        private void ThrowIfReadonly()
        {
            if (this.IsReadOnly)
            {
                throw new ImmutableException(this, $"This {this.GetType()} is immutable and cannot be modified.");
            }
        }

        /// <summary>
        /// Throws an exception  <see cref="ProxyList{T}"/> is revoked.
        /// </summary>
        private void ThrowIfRevoked()
        {
            if (this.collectionDraftState?.Revoked ?? false)
            {
                throw new DraftException(this, $"This {this.GetType()} instance is revoked.");
            }
        }

        /// <summary>
        /// Executes the Copy on write on the inner list.
        /// </summary>
        private void CopyOnWrite()
        {
            if (ReferenceEquals(this.Original, this.InnerList))
            {
                this.InnerList = new List<T>(this.Original);
            }
        }
    }
}
