// <copyright file="ProxyDictionary.cs" company="Jan-Willem Spuij">
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
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Text;
    using RedCow.Collections;

    /// <summary>
    /// A dictionary that can act as a proxy around another dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys held in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values held in the dictionary.</typeparam>
    public class ProxyDictionary<TKey, TValue>
        : ProxyCollectionBase<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>, IDraft, ILockable,
        ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable,
        IDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>,
        IReadOnlyDictionary<TKey, TValue>, ICollection, IDictionary

        // todo add serialization, IDeserializationCallback, ISerializable
    {
        /// <summary>
        /// The values collection wrapper.
        /// </summary>
        private ProxyValueCollection? values;

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        public ICollection<TKey> Keys => this.InnerCollection.Keys;

        /// <summary>
        /// Gets a collection containing the values in the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        public ICollection<TValue> Values => this.values ??= new ProxyValueCollection(this);

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.InnerCollection.Keys;

        /// <summary>
        /// Gets a collection containing the values in the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.values ??= new ProxyValueCollection(this);

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        ICollection IDictionary.Keys => this.InnerCollection.Keys;

        /// <summary>
        /// Gets a collection containing the values in the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        ICollection IDictionary.Values => this.values ??= new ProxyValueCollection(this);

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>
        /// The value associated with the specified key. If the specified key is not found,
        /// a get operation throws a System.Collections.Generic.KeyNotFoundException, and
        /// a set operation creates a new element with the specified key.
        /// </returns>
        object? IDictionary.this[object key]
        {
            get => this[(TKey)key];
            set => this[(TKey)key] = (TValue)value!;
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>
        /// The value associated with the specified key. If the specified key is not found,
        /// a get operation throws a System.Collections.Generic.KeyNotFoundException, and
        /// a set operation creates a new element with the specified key.
        /// </returns>
        public TValue this[TKey key]
        {
            get
            {
                this.ThrowIfRevoked();

                if (this.Locked || this.CollectionDraftState == null)
                {
                    return this.InnerCollection[key];
                }

                return this.CollectionDraftState!.Get(() => this.InnerCollection[key], value => this.InnerCollection[key] = value, this.CopyOnWrite);
            }

            set
            {
                this.ThrowIfReadonly();
                this.CollectionDraftState!.Modify(() => this.InnerCollection[key] = value, this.CopyOnWrite);
            }
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        public void Add(TKey key, TValue value)
        {
            this.ThrowIfReadonly();
            this.CollectionDraftState!.Modify(() => this.InnerCollection.Add(key, value), this.CopyOnWrite);
        }

        /// <summary>
        /// Determines whether the <see cref="ProxyDictionary{TKey, TValue}"/> contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="ProxyDictionary{TKey, TValue}"/>.</param>
        /// <returns>true if the <see cref="ProxyDictionary{TKey, TValue}"/> contains an element with the specified key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            this.ThrowIfRevoked();
            return this.InnerCollection.ContainsKey(key);
        }

        /// <summary>
        /// Removes the value with the specified key from the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully found and removed; otherwise, false. This
        /// method returns false if key is not found in the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </returns>
        public bool Remove(TKey key)
        {
            this.ThrowIfReadonly();
            bool result = false;
            this.CollectionDraftState!.Modify(() => result = this.InnerCollection.Remove(key), this.CopyOnWrite);
            return result;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the
        /// key is found; otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements <see cref="ProxyDictionary{TKey, TValue}"/> contains
        /// an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            this.ThrowIfRevoked();

            var result = this.InnerCollection.TryGetValue(key, out TValue source);

            if (this.Locked || this.CollectionDraftState == null)
            {
                value = source;
            }
            else
            {
                value = this.CollectionDraftState!.Get(() => source, v => this.InnerCollection[key] = v, this.CopyOnWrite);
            }

            return result;
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        void IDictionary.Add(object key, object value)
        {
            this.Add((TKey)key, (TValue)value);
        }

        /// <summary>
        ///  Removes all elements from the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        void IDictionary.Clear()
        {
            this.Clear();
        }

        /// <summary>
        /// Determines whether the System.Collections.Generic.Dictionary`2 contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the System.Collections.Generic.Dictionary`2.</param>
        /// <returns>true if the System.Collections.Generic.Dictionary`2 contains an element with the specified key; otherwise, false.</returns>
        bool IDictionary.Contains(object key)
        {
            return this.ContainsKey((TKey)key);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ProxyDictionary{TKey, TValue}"/>.</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new ProxyDictionaryEnumerator(this.GetEnumerator());
        }

        /// <summary>
        /// Removes the value with the specified key from the System.Collections.Generic.Dictionary`2.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        void IDictionary.Remove(object key)
        {
            this.Remove((TKey)key);
        }

        /// <summary>
        /// Gets an enumerator that creates drafts while enumerating.
        /// </summary>
        /// <param name="enumerator">The source enumerator.</param>
        /// <returns>An enumerator that creates drafts.</returns>
        protected override IEnumerator<KeyValuePair<TKey, TValue>> GetDraftEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
        {
            while (enumerator.MoveNext())
            {
                var original = enumerator.Current;
                yield return new KeyValuePair<TKey, TValue>(original.Key, this.CollectionDraftState!.Get(() => original.Value, value => this.InnerCollection[original.Key] = value, this.CopyOnWrite));
            }
        }

        /// <summary>
        /// Executes the Copy on write on the inner list.
        /// </summary>
        protected override void CopyOnWrite()
        {
            if (ReferenceEquals(this.Original, this.InnerCollection))
            {
                this.InnerCollection = new Dictionary<TKey, TValue>(this.Original);
            }
        }

        /// <summary>
        /// Gets an enumerator of the inner values.
        /// </summary>
        /// <returns>An enumerator with inner values.</returns>
        protected override System.Collections.IEnumerator GetInnerValueEnumerator()
        {
            return this.InnerCollection.Values.GetEnumerator();
        }

        /// <summary>
        /// Nongeneric enumerator wrapper for <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        private sealed class ProxyDictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> enumerator;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxyDictionaryEnumerator"/> class.
            /// </summary>
            /// <param name="enumerator">The enumerator to wrap.</param>
            public ProxyDictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                this.enumerator = enumerator;
            }

            /// <summary>
            /// Gets the Dictionary Entry.
            /// </summary>
            public DictionaryEntry Entry => new DictionaryEntry(this.enumerator.Current.Key, this.enumerator.Current.Value);

            /// <summary>
            /// Gets the key.
            /// </summary>
            public object? Key => this.enumerator.Current.Key;

            /// <summary>
            /// Gets the value.
            /// </summary>
            public object? Value => this.enumerator.Current.Value;

            /// <summary>
            /// Gets the current item.
            /// </summary>
            public object Current => this.enumerator.Current;

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>True when the advancement succeeded, false otherwise.</returns>
            public bool MoveNext()
            {
                return this.enumerator.MoveNext();
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element
            /// in the collection.
            /// </summary>
            public void Reset()
            {
                this.enumerator.Reset();
            }
        }

        /// <summary>
        /// Wraps the collection of values inside the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        private sealed class ProxyValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
        {
            private readonly ProxyDictionary<TKey, TValue> dictionary;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxyValueCollection"/> class.
            /// </summary>
            /// <param name="dictionary">The dictionary.</param>
            public ProxyValueCollection(ProxyDictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
            }

            /// <summary>
            /// Gets the number of elements contained in the collection.
            /// </summary>
            public int Count => this.dictionary.Count;

            /// <summary>
            /// Gets a value indicating whether the collection is read only.
            /// </summary>
            bool ICollection<TValue>.IsReadOnly => true;

            /// <summary>
            /// Gets a value indicating whether access to the collection is synchronized (thread safe).
            /// </summary>
            bool ICollection.IsSynchronized => this.dictionary.IsSynchronized;

            /// <summary>
            /// Gets an object that can be used to synchronize access to the collection.
            /// </summary>
            object ICollection.SyncRoot => ((ICollection)this.dictionary).SyncRoot;

            /// <summary>
            /// Adds an item to the collection.
            /// </summary>
            /// <param name="item">The item to add.</param>
            void ICollection<TValue>.Add(TValue item)
                => this.dictionary.Values.Add(item);

            /// <summary>
            /// Removes the first occurrence of a specific object from the collection.
            /// </summary>
            /// <param name="item">The object to remove from the collection.</param>
            /// <returns>
            /// true if item was successfully removed from the collection;
            /// otherwise, false. This method also returns false if item is not found in the
            /// original collection.</returns>
            bool ICollection<TValue>.Remove(TValue item)
                => this.dictionary.Values.Remove(item);

            /// <summary>
            ///  Removes all items from the collection.
            /// </summary>
            void ICollection<TValue>.Clear()
                => this.dictionary.Values.Clear();

            /// <summary>
            /// Determines whether the collection contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the collection.</param>
            /// <returns>true if item is found in the collection; otherwise, false.</returns>
            bool ICollection<TValue>.Contains(TValue item)
                => this.dictionary.Values.Contains(item);

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
                => this.GetEnumerator();

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<TValue> GetEnumerator()
            {
                this.dictionary.ThrowIfRevoked();

                if (this.dictionary.Locked || this.dictionary.CollectionDraftState == null)
                {
                    return this.dictionary.InnerCollection.Values.GetEnumerator();
                }

                return this.GetDraftEnumerator(this.dictionary.InnerCollection.GetEnumerator());
            }

            /// <summary>
            /// Copies the elements of the collection. to an System.Array,
            /// starting at a particular System.Array index.
            /// </summary>
            /// <param name="array">The destination array.</param>
            /// <param name="arrayIndex">The array index.</param>
            public void CopyTo(TValue[] array, int arrayIndex)
            {
                this.dictionary.ThrowIfRevoked();
                this.dictionary.InnerCollection.Values.CopyTo(array, arrayIndex);
            }

            /// <summary>
            /// Copies the elements of the collection. to an System.Array,
            /// starting at a particular System.Array index.
            /// </summary>
            /// <param name="array">The destination array.</param>
            /// <param name="index">The array index.</param>
            void ICollection.CopyTo(Array array, int index)
            {
                this.dictionary.ThrowIfRevoked();
                ((ICollection)this.dictionary.InnerCollection.Values).CopyTo(array, index);
            }

            /// <summary>
            /// Gets an enumerator that creates drafts while enumerating.
            /// </summary>
            /// <param name="enumerator">The source enumerator.</param>
            /// <returns>An enumerator that creates drafts.</returns>
            private IEnumerator<TValue> GetDraftEnumerator(Dictionary<TKey, TValue>.Enumerator enumerator)
            {
                while (enumerator.MoveNext())
                {
                    var original = enumerator.Current;
                    yield return this.dictionary.CollectionDraftState!.Get(() => original.Value, value => this.dictionary.InnerCollection[original.Key] = value, this.dictionary.CopyOnWrite);
                }
            }
        }
    }
}
