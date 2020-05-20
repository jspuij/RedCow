// <copyright file="StoreTests.cs" company="Jan-Willem Spuij">
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

namespace RedCow.X.Test
{
    using System;
    using System.Collections.Generic;
    using RedCow.Test;
    using Xunit;

    /// <summary>
    /// Unit tests for the store.
    /// </summary>
    public class StoreTests
    {
        /// <summary>
        /// Tests that Dispatch of an action works.
        /// </summary>
        [Fact]
        public void DispatchTest()
        {
            TestPerson initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
            };

            var reducer = ITestPerson.Producer<object>((person, action) =>
            {
                switch (action)
                {
                    case "ChangeGender":
                        person.FirstName = "Jane";
                        break;
                }
            });

            var store = new Store<ITestPerson>(initial, reducer);
            store.Dispatch("ChangeGender");

            Assert.NotSame(initial, store.State);
            Assert.Equal("Jane", store.State.FirstName);
        }

        /// <summary>
        /// Tests registering an observer to the store.
        /// </summary>
        [Fact]
        public void ObserverTest()
        {
            TestPerson initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
            };

            var reducer = ITestPerson.Producer<object>((person, action) =>
            {
                switch (action)
                {
                    case "ChangeGender":
                        person.FirstName = "Jane";
                        break;
                }
            });

            var store = new Store<ITestPerson>(initial, reducer);
            var observer = new StoreObserver();

            using var disposable = store.Subscribe(observer);
            store.Dispatch("ChangeGender");

            Assert.Collection(observer.Values, t =>
            {
                Assert.NotSame(initial, t);
                Assert.Equal("Jane", t.FirstName);
            });
        }

        /// <summary>
        /// Mini observer class to test the observables.
        /// </summary>
        private class StoreObserver : IObserver<ITestPerson>
        {
            private readonly List<ITestPerson> values = new List<ITestPerson>();

            /// <summary>
            /// Gets the values that were observed.
            /// </summary>
            public IReadOnlyList<ITestPerson> Values { get => this.values; }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            /// <summary>
            /// Provides the observer with new data.
            /// Stores dat in values list.
            /// </summary>
            /// <param name="value">The provided value.</param>
            public void OnNext(ITestPerson value)
            {
                this.values.Add(value);
            }
        }
    }
}
