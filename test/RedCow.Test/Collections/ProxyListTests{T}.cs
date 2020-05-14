// <copyright file="ProxyListTests{T}.cs" company="Jan-Willem Spuij">
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
    using System.Text;
    using RedCow.Immutable;
    using RedCow.Immutable.Collections;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="ProxyList{T}"/> class.
    /// </summary>
    /// <typeparam name="T">The type inside the proxylist.</typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class ProxyListTests<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyListTests{T}"/> class.
        /// </summary>
        /// <param name="tCreator">The function to create a new T.</param>
        public ProxyListTests(Func<T> tCreator)
        {
            this.DraftScope = DraftExtensions.CreateDraft(new ProxyList<T>(), out var result);
            this.ProxyList = result;
            this.TCreator = tCreator;
        }

        /// <summary>
        /// Gets proxy list.
        /// </summary>
        protected ProxyList<T> ProxyList { get; private set; }

        /// <summary>
        /// Gets draft scope.
        /// </summary>
        protected IDraftScope DraftScope { get; private set; }

        /// <summary>
        /// Gets creator function.
        /// </summary>
        protected Func<T> TCreator { get; private set; }

        /// <summary>
        /// Tests the Add method.
        /// </summary>
        [Fact]
        public void AddTest()
        {
            var testValue = this.TCreator();
            this.ProxyList.Add(testValue);
            var result = this.FinishDraft();
            Assert.Equal(1, result.Count);
            Assert.Equal<T>(testValue, result[0]);
        }

        /// <summary>
        /// Tests the Update method.
        /// </summary>
        [Fact]
        public void UpdateTest()
        {
            var testValue1 = this.TCreator();
            var testValue2 = this.TCreator();
            this.ProxyList.Add(testValue1);
            this.ProxyList[0] = testValue2;
            var result = this.FinishDraft();
            Assert.Equal(1, result.Count);
            Assert.Equal<T>(testValue2, result[0]);
        }

        /// <summary>
        /// Tests the Clear method.
        /// </summary>
        [Fact]
        public void ClearTest()
        {
            var testValue1 = this.TCreator();
            var testValue2 = this.TCreator();
            var testValue3 = this.TCreator();

            this.ProxyList.Add(testValue1);
            this.ProxyList.Add(testValue2);
            this.ProxyList.Add(testValue3);

            Assert.Equal(3, this.ProxyList.Count);

            this.ProxyList.Clear();

            var result = this.FinishDraft();
            Assert.Equal(0, result.Count);
        }

        /// <summary>
        /// Tests the Clear method.
        /// </summary>
        [Fact]
        public void ContainsTest()
        {
            var testValue = this.TCreator();
            this.ProxyList.Add(testValue);
            var result = this.FinishDraft();
            Assert.Contains(testValue, result);
        }

        /// <summary>
        /// Tests the CopyTo method.
        /// </summary>
        [Fact]
        public void CopyToTest()
        {
            var testValue1 = this.TCreator();
            var testValue2 = this.TCreator();
            var testValue3 = this.TCreator();

            this.ProxyList.Add(testValue1);
            this.ProxyList.Add(testValue2);
            this.ProxyList.Add(testValue3);

            var proxyList = this.FinishDraft();

            var result = new T[3];

            ((ICollection<T>)proxyList).CopyTo(result, 0);

            Assert.Equal(result, proxyList);
        }

        /// <summary>
        /// Tests the generic enumerator.
        /// </summary>
        [Fact]
        public void GenericEnumeratorTest()
        {
            var testValue1 = this.TCreator();
            var testValue2 = this.TCreator();
            var testValue3 = this.TCreator();

            this.ProxyList.Add(testValue1);
            this.ProxyList.Add(testValue2);
            this.ProxyList.Add(testValue3);

            var proxyList = this.FinishDraft();

            int index = 0;
            foreach (T value in proxyList)
            {
                Assert.Equal(proxyList[index++], value);
            }
        }

        /// <summary>
        /// Tests the non-generic enumerator.
        /// </summary>
        [Fact]
        public void NonGenericEnumeratorTest()
        {
            var testValue1 = this.TCreator();
            var testValue2 = this.TCreator();
            var testValue3 = this.TCreator();

            this.ProxyList.Add(testValue1);
            this.ProxyList.Add(testValue2);
            this.ProxyList.Add(testValue3);

            var proxyList = this.FinishDraft();

            int index = 0;
            foreach (object? value in (IEnumerable)proxyList)
            {
                Assert.Equal(proxyList[index++], value);
            }
        }

        /// <summary>
        /// Tests the IndexOf method.
        /// </summary>
        [Fact]
        public void IndexOfTest()
        {
            var testValue1 = this.TCreator();
            var testValue2 = this.TCreator();
            var testValue3 = this.TCreator();

            this.ProxyList.Add(testValue1);
            this.ProxyList.Add(testValue2);
            this.ProxyList.Add(testValue3);

            var proxyList = this.FinishDraft();

            int index = 0;
            foreach (T value in proxyList)
            {
                Assert.Equal(index++, ((IList<T>)proxyList).IndexOf(value));
            }
        }

        /// <summary>
        /// Tests the Insert method.
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            var testValue1 = this.TCreator();
            var testValue2 = this.TCreator();
            var testValue3 = this.TCreator();

            this.ProxyList.Insert(0, testValue1);
            this.ProxyList.Insert(0, testValue2);
            this.ProxyList.Insert(0, testValue3);

            var result = this.FinishDraft();

            Assert.Collection(
                result,
                v => Assert.Equal(testValue3, v),
                v => Assert.Equal(testValue2, v),
                v => Assert.Equal(testValue1, v));
        }

        /// <summary>
        /// Tests the Is... properties on the <see cref="ProxyList{T}"/>.
        /// </summary>
        [Fact]
        public void IsPropertiesTest()
        {
            // we need to add a single Item or it will be revoked.
            var testValue = this.TCreator();
            this.ProxyList.Add(testValue);

            Assert.False(this.ProxyList.IsFixedSize);
            Assert.False(this.ProxyList.IsReadOnly);
            Assert.False(this.ProxyList.IsSynchronized);

            this.FinishDraft();

            Assert.True(this.ProxyList.IsFixedSize);
            Assert.True(this.ProxyList.IsReadOnly);
            Assert.True(this.ProxyList.IsSynchronized);
        }

        /// <summary>
        /// Tests for the original.
        /// </summary>
        [Fact]
        public void OriginalTest()
        {
            // we need to add a single Item or it will be revoked.
            var testValue = this.TCreator();
            this.ProxyList.Add(testValue);
            var original = this.ProxyList.Original;
            Assert.NotNull(original);

            var result = this.FinishDraft();

            Assert.Null(((ProxyList<T>)result).Original);
        }

        /// <summary>
        /// Tests the Remove method.
        /// </summary>
        [Fact]
        public void RemoveTest()
        {
            var testValue1 = this.TCreator();
            var testValue2 = this.TCreator();

            this.ProxyList.Add(testValue1);
            this.ProxyList.Add(testValue2);
            this.ProxyList.Remove(testValue1);
            var result = this.FinishDraft();
            Assert.Equal(1, result.Count);
            Assert.Equal(testValue2, result[0]);
        }

        /// <summary>
        /// Tests the RemoveAt method.
        /// </summary>
        [Fact]
        public void RemoveAtTest()
        {
            var testValue1 = this.TCreator();
            var testValue2 = this.TCreator();

            this.ProxyList.Add(testValue1);
            this.ProxyList.Add(testValue2);
            this.ProxyList.RemoveAt(0);

            var result = this.FinishDraft();
            Assert.Equal(1, result.Count);
            Assert.Equal(testValue2, result[0]);
        }

        /// <summary>
        /// Tests all operations for readonly.
        /// </summary>
        [Fact]
        public void ReadOnlyTest()
        {
            var testValue = this.TCreator();
            this.ProxyList.Add(testValue);
            var result = this.FinishDraft();

            Assert.Throws<ImmutableException>(() => ((ProxyList<T>)result).Add(testValue));
            Assert.Throws<ImmutableException>(() => ((ProxyList<T>)result)[0] = testValue);
            Assert.Throws<ImmutableException>(() => ((ProxyList<T>)result).Clear());
            Assert.Throws<ImmutableException>(() => ((ProxyList<T>)result).Insert(0, testValue));
            Assert.Throws<ImmutableException>(() => ((ProxyList<T>)result).Remove(testValue));
            Assert.Throws<ImmutableException>(() => ((ProxyList<T>)result).RemoveAt(0));
        }

        /// <summary>
        /// Tests all operations for revoked.
        /// </summary>
        [Fact]
        public void RevokedTest()
        {
            var testValue = this.TCreator();

            // don't add the testvalue. The draft collection will be revoked.
            var result = this.FinishDraft();

            Assert.Throws<DraftRevokedException>(() => this.ProxyList.Add(testValue));
            Assert.Throws<DraftRevokedException>(() => Assert.Equal(this.ProxyList[0], testValue));
            Assert.Throws<DraftRevokedException>(() => this.ProxyList[0]);
            Assert.Throws<DraftRevokedException>(() => this.ProxyList.Contains(testValue));
            Assert.Throws<DraftRevokedException>(() => this.ProxyList.Clear());
            Assert.Throws<DraftRevokedException>(() => this.ProxyList.Count);
            Assert.Throws<DraftRevokedException>(() => this.ProxyList.GetEnumerator().MoveNext());
            Assert.Throws<DraftRevokedException>(() => this.ProxyList.IndexOf(testValue));
            Assert.Throws<DraftRevokedException>(() => this.ProxyList.Insert(0, testValue));
            Assert.Throws<DraftRevokedException>(() => this.ProxyList.Original);
            Assert.Throws<DraftRevokedException>(() => this.ProxyList.Remove(testValue));
            Assert.Throws<DraftRevokedException>(() => this.ProxyList.RemoveAt(0));
        }

        /// <summary>
        /// Finishes the draft for this list.
        /// </summary>
        /// <returns>A readonly list.</returns>
        private IReadOnlyList<T> FinishDraft()
        {
            return this.DraftScope.FinishDraft<ProxyList<T>, IReadOnlyList<T>>(this.ProxyList);
        }
    }

    /// <summary>
    /// Unit tests for the <see cref="ProxyList{T}"/> class with TestPerson draftable content.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Unit tests of single type in single file.")]
    public class ProxyListTestPersonTests : ProxyListTests<TestPerson>
    {
        private static int count = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyListTestPersonTests"/> class.
        /// </summary>
        public ProxyListTestPersonTests()
            : base(() => new TestPerson()
            {
                LastName = $"Testperson{count++}",
            })
        {
        }
    }

    /// <summary>
    /// Unit tests for the <see cref="ProxyList{T}"/> class with TestPerson draftable content.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Unit tests of single type in single file.")]
    public class ProxyListStringTests : ProxyListTests<string>
    {
        private static int count = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyListStringTests"/> class.
        /// </summary>
        public ProxyListStringTests()
            : base(() => $"TestString{count++}")
        {
        }
    }

    /// <summary>
    /// Unit tests for the <see cref="ProxyList{T}"/> class with TestPerson draftable content.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Unit tests of single type in single file.")]
    public class ProxyListIntegerTests : ProxyListTests<int>
    {
        private static int count = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyListIntegerTests"/> class.
        /// </summary>
        public ProxyListIntegerTests()
            : base(() => count++)
        {
        }
    }
}
