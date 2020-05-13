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
    public class ProxyList<T>
        : ProxyCollectionBase<List<T>, T>, IDraft, ILockable, ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
    {
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

                if (this.Locked || this.CollectionDraftState == null)
                {
                    return this.InnerCollection[index];
                }

                return this.CollectionDraftState!.Get(() => this.InnerCollection[index], value => this.InnerCollection[index] = value, this.CopyOnWrite);
            }

            set
            {
                this.ThrowIfReadonly();
                this.CollectionDraftState!.Modify(() => this.InnerCollection[index] = value, this.CopyOnWrite);
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
        /// Determines the index of a specific item in the <see cref="ProxyList{T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProxyList{T}"/>.</param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            this.ThrowIfRevoked();
            return this.InnerCollection.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="ProxyList{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="ProxyList{T}"/>.</param>
        public void Insert(int index, T item)
        {
            this.ThrowIfReadonly();
            this.CollectionDraftState!.Modify(() => this.InnerCollection.Insert(index, item), this.CopyOnWrite);
        }

        /// <summary>
        /// Removes the <see cref="ProxyList{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            this.ThrowIfReadonly();
            this.CollectionDraftState!.Modify(() => this.InnerCollection.RemoveAt(index), this.CopyOnWrite);
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
            this.CollectionDraftState!.Modify(() => index = ((IList)this.InnerCollection).Add(value), this.CopyOnWrite);
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
        /// Executes the Copy on write on the inner list.
        /// </summary>
        protected override void CopyOnWrite()
        {
            if (ReferenceEquals(this.Original, this.InnerCollection))
            {
                this.InnerCollection = new List<T>(this.Original);
            }
        }

        /// <summary>
        /// Gets an enumerator that creates drafts while enumerating.
        /// </summary>
        /// <param name="enumerator">The source enumerator.</param>
        /// <returns>An enumerator that creates drafts.</returns>
        protected override IEnumerator<T> GetDraftEnumerator(IEnumerator<T> enumerator)
        {
            int index = 0;
            while (enumerator.MoveNext())
            {
                try
                {
                    yield return this.CollectionDraftState!.Get(() => enumerator.Current, value => this.InnerCollection[index] = value, this.CopyOnWrite);
                }
                finally
                {
                    index++;
                }
            }
        }

        /// <summary>
        /// Gets an enumerator of the inner values.
        /// </summary>
        /// <returns>An enumerator with inner values.</returns>
        protected override IEnumerator GetInnerValueEnumerator()
        {
            return this.InnerCollection.GetEnumerator();
        }
    }
}
