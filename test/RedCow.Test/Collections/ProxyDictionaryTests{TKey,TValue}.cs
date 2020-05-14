// <copyright file="ProxyDictionaryTests{TKey,TValue}.cs" company="Jan-Willem Spuij">
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

namespace RedCow.Test.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using RedCow.Immutable;
    using RedCow.Immutable.Collections;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="ProxyDictionaryTests{TKey, TValue}"/> class.
    /// </summary>
    /// <typeparam name="TKey">The type for keys inside the proxydictionary.</typeparam>
    /// <typeparam name="TValue">The type for values inside the proxydictionary.</typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class ProxyDictionaryTests<TKey, TValue>
        where TKey : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyDictionaryTests{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="keyValuePairCreator">The function to create a new T.</param>
        public ProxyDictionaryTests(Func<KeyValuePair<TKey, TValue>> keyValuePairCreator)
        {
            this.DraftScope = DraftExtensions.CreateDraft(new ProxyDictionary<TKey, TValue>(), out var result);
            this.ProxyDictionary = result;
            this.KeyValuePairCreator = keyValuePairCreator;
        }

        /// <summary>
        /// Gets proxy dictionary.
        /// </summary>
        protected ProxyDictionary<TKey, TValue> ProxyDictionary { get; private set; }

        /// <summary>
        /// Gets draft scope.
        /// </summary>
        protected IDraftScope DraftScope { get; private set; }

        /// <summary>
        /// Gets creator function.
        /// </summary>
        protected Func<KeyValuePair<TKey, TValue>> KeyValuePairCreator { get; private set; }

        /// <summary>
        /// Tests the Add method.
        /// </summary>
        [Fact]
        public void AddTest()
        {
            var testValue = this.KeyValuePairCreator();
            this.ProxyDictionary.Add(testValue);
            var result = this.FinishDraft();
            Assert.Equal(1, result.Count);
            Assert.Equal(testValue, result.First());
        }

        /// <summary>
        /// Tests the Decomposed Add method.
        /// </summary>
        [Fact]
        public void DecomposedAddTest()
        {
            var testValue = this.KeyValuePairCreator();
            this.ProxyDictionary.Add(testValue.Key, testValue.Value);
            var result = this.FinishDraft();
            Assert.Equal(1, result.Count);
            Assert.Equal(testValue, result.First());
        }

        /// <summary>
        /// Tests the Clear method.
        /// </summary>
        [Fact]
        public void ClearTest()
        {
            var testValue1 = this.KeyValuePairCreator();
            var testValue2 = this.KeyValuePairCreator();
            var testValue3 = this.KeyValuePairCreator();

            this.ProxyDictionary.Add(testValue1);
            this.ProxyDictionary.Add(testValue2);
            this.ProxyDictionary.Add(testValue3);

            Assert.Equal(3, this.ProxyDictionary.Count);

            this.ProxyDictionary.Clear();

            var result = this.FinishDraft();
            Assert.Equal(0, result.Count);
        }

        /// <summary>
        /// Tests the Clear method.
        /// </summary>
        [Fact]
        public void ContainsTest()
        {
            var testValue = this.KeyValuePairCreator();
            this.ProxyDictionary.Add(testValue);
            var result = this.FinishDraft();
            Assert.Contains(testValue, result);
        }

        /// <summary>
        /// Tests the Clear method.
        /// </summary>
        [Fact]
        public void ContainsKeyTest()
        {
            var testValue = this.KeyValuePairCreator();
            this.ProxyDictionary.Add(testValue);
            var result = this.FinishDraft();
            Assert.True(result.ContainsKey(testValue.Key));
        }

        /// <summary>
        /// Tests the CopyTo method.
        /// </summary>
        [Fact]
        public void CopyToTest()
        {
            var testValue1 = this.KeyValuePairCreator();
            var testValue2 = this.KeyValuePairCreator();
            var testValue3 = this.KeyValuePairCreator();

            this.ProxyDictionary.Add(testValue1);
            this.ProxyDictionary.Add(testValue2);
            this.ProxyDictionary.Add(testValue3);

            var proxyDictionary = this.FinishDraft();

            var result = new KeyValuePair<TKey, TValue>[3];

            ((ICollection<KeyValuePair<TKey, TValue>>)proxyDictionary).CopyTo(result, 0);

            Assert.Equal(result, proxyDictionary);
        }

        /// <summary>
        /// Tests the generic enumerator.
        /// </summary>
        [Fact]
        public void GenericEnumeratorTest()
        {
            var testValue1 = this.KeyValuePairCreator();
            var testValue2 = this.KeyValuePairCreator();
            var testValue3 = this.KeyValuePairCreator();

            this.ProxyDictionary.Add(testValue1);
            this.ProxyDictionary.Add(testValue2);
            this.ProxyDictionary.Add(testValue3);

            var proxyDictionary = this.FinishDraft();

            foreach (KeyValuePair<TKey, TValue> value in proxyDictionary)
            {
                Assert.Equal(proxyDictionary[value.Key], value.Value);
            }
        }

        /// <summary>
        /// Tests the non-generic enumerator.
        /// </summary>
        [Fact]
        public void NonGenericEnumeratorTest()
        {
            var testValue1 = this.KeyValuePairCreator();
            var testValue2 = this.KeyValuePairCreator();
            var testValue3 = this.KeyValuePairCreator();

            this.ProxyDictionary.Add(testValue1);
            this.ProxyDictionary.Add(testValue2);
            this.ProxyDictionary.Add(testValue3);

            var proxyDictionary = this.FinishDraft();

#pragma warning disable CS8605 // Unboxing a possibly null value.
            foreach (DictionaryEntry value in (IDictionary)proxyDictionary)
#pragma warning restore CS8605 // Unboxing a possibly null value.
            {
                Assert.Equal(proxyDictionary[(TKey)value.Key], value.Value);
            }
        }

        /// <summary>
        /// Tests the Is... properties on the <see cref="ProxyDictionary{TKey, TValue}"/>.
        /// </summary>
        [Fact]
        public void IsPropertiesTest()
        {
            // we need to add a single Item or it will be revoked.
            var testValue = this.KeyValuePairCreator();
            this.ProxyDictionary.Add(testValue);

            Assert.False(this.ProxyDictionary.IsFixedSize);
            Assert.False(this.ProxyDictionary.IsReadOnly);
            Assert.False(this.ProxyDictionary.IsSynchronized);

            this.FinishDraft();

            Assert.True(this.ProxyDictionary.IsFixedSize);
            Assert.True(this.ProxyDictionary.IsReadOnly);
            Assert.True(this.ProxyDictionary.IsSynchronized);
        }

        /// <summary>
        /// Tests for the original.
        /// </summary>
        [Fact]
        public void OriginalTest()
        {
            // we need to add a single Item or it will be revoked.
            var testValue = this.KeyValuePairCreator();
            this.ProxyDictionary.Add(testValue);
            var original = this.ProxyDictionary.Original;
            Assert.NotNull(original);

            var result = this.FinishDraft();

            Assert.Null(((ProxyDictionary<TKey, TValue>)result).Original);
        }

        /// <summary>
        /// Tests the Remove method.
        /// </summary>
        [Fact]
        public void RemoveTest()
        {
            var testValue1 = this.KeyValuePairCreator();
            var testValue2 = this.KeyValuePairCreator();

            this.ProxyDictionary.Add(testValue1);
            this.ProxyDictionary.Add(testValue2);
            this.ProxyDictionary.Remove(testValue1);
            var result = this.FinishDraft();
            Assert.Equal(1, result.Count);
            Assert.Equal(testValue2, result.First());
        }

        /// <summary>
        /// Tests the Remove method with key.
        /// </summary>
        [Fact]
        public void RemoveWithKeyTest()
        {
            var testValue1 = this.KeyValuePairCreator();
            var testValue2 = this.KeyValuePairCreator();

            this.ProxyDictionary.Add(testValue1);
            this.ProxyDictionary.Add(testValue2);
            this.ProxyDictionary.Remove(testValue1.Key);
            var result = this.FinishDraft();
            Assert.Equal(1, result.Count);
            Assert.Equal(testValue2, result.First());
        }

        /// <summary>
        /// Finishes the draft for this dictionary.
        /// </summary>
        /// <returns>A readonly dictionary.</returns>
        private IReadOnlyDictionary<TKey, TValue> FinishDraft()
        {
            return this.DraftScope.FinishDraft<ProxyDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>>(this.ProxyDictionary);
        }
    }

    /// <summary>
    /// Unit tests for the <see cref="ProxyDictionary{TKey, TValue}"/> class with TestPerson draftable content.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Unit tests of single type in single file.")]
    public class ProxyDictionaryTestPersonTests : ProxyDictionaryTests<string, TestPerson>
    {
        private static int count = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyDictionaryTestPersonTests"/> class.
        /// </summary>
        public ProxyDictionaryTestPersonTests()
            : base(() => new KeyValuePair<string, TestPerson>($"TestKey{count}", new TestPerson()
            {
                LastName = $"Testperson{count++}",
            }))
        {
        }
    }

    /// <summary>
    /// Unit tests for the <see cref="ProxyDictionary{TKey, TValue}"/> class with TestPerson draftable content.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Unit tests of single type in single file.")]
    public class ProxyDictionaryStringTests : ProxyDictionaryTests<string, string>
    {
        private static int count = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyDictionaryStringTests"/> class.
        /// </summary>
        public ProxyDictionaryStringTests()
            : base(() => new KeyValuePair<string, string>($"TestKey{count}", $"TestString{count++}"))
        {
        }
    }

    /// <summary>
    /// Unit tests for the <see cref="ProxyDictionary{TKey, TValue}"/> class with TestPerson draftable content.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Unit tests of single type in single file.")]
    public class ProxyDictionaryIntegerTests : ProxyDictionaryTests<string, int>
    {
        private static int count = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyDictionaryIntegerTests"/> class.
        /// </summary>
        public ProxyDictionaryIntegerTests()
            : base(() => new KeyValuePair<string, int>($"TestKey{count}", count++))
        {
        }
    }
}
