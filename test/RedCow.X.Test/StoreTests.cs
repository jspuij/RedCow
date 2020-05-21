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
    using System.Diagnostics.CodeAnalysis;
    using RedCow.Test;
    using Xunit;

    /// <summary>
    /// Unit tests for the store.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
                    case "ChangeFirstName":
                        person.FirstName = "Jane";
                        break;
                }
            });

            var store = new Store<ITestPerson>(initial, reducer);
            store.Dispatch("ChangeFirstName");

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
                    case "ChangeFirstName":
                        person.FirstName = "Jane";
                        break;
                }
            });

            var store = new Store<ITestPerson>(initial, reducer);

            bool observed = false;

            var observer = new DelegateObserver<ITestPerson>(value =>
            {
                observed = true;
                Assert.NotSame(initial, value);
                Assert.Equal("Jane", value.FirstName);
            });

            using var disposable = store.Subscribe(observer);
            store.Dispatch("ChangeFirstName");

            Assert.True(observed);
        }

        /// <summary>
        /// Tests that unsubscribing during observe doesn not throw.
        /// </summary>
        [Fact]
        public void UnsubscribeDuringObserve()
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
                    case "ChangeFirstName":
                        person.FirstName = "Jane";
                        break;
                }
            });

            var store = new Store<ITestPerson>(initial, reducer);

            IDisposable? disposable = null;

            var observer = new DelegateObserver<ITestPerson>(value =>
            {
                disposable!.Dispose();
            });

            disposable = store.Subscribe(observer);

            // should not throw.
            store.Dispatch("ChangeFirstName");
        }

        /// <summary>
        /// Tests that unsubscribing during dispatch throws.
        /// </summary>
        [Fact]
        public void UnsubscribeDuringDispathThrows()
        {
            TestPerson initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
            };

            IDisposable? disposable = null;

            var reducer = ITestPerson.Producer<object>((person, action) =>
            {
                switch (action)
                {
                    case "Unsubscribe":
                        disposable!.Dispose();
                        break;
                }
            });

            var store = new Store<ITestPerson>(initial, reducer);

            var observer = new DelegateObserver<ITestPerson>(value =>
            {
                Assert.True(false);
            });

            disposable = store.Subscribe(observer);

            // should throw.
            Assert.Throws<DispatchException>(() => store.Dispatch("Unsubscribe"));
        }

        /// <summary>
        /// Tests that subscribing during dispatch throws.
        /// </summary>
        [Fact]
        public void SubscribeDuringDispathThrows()
        {
            TestPerson initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
            };

            IDisposable? disposable = null;
            Store<ITestPerson>? store = null;

            var observer = new DelegateObserver<ITestPerson>(value =>
            {
                Assert.True(false);
            });

            var reducer = ITestPerson.Producer<object>((person, action) =>
            {
                switch (action)
                {
                    case "Subscribe":
                        disposable = store!.Subscribe(observer);
                        break;
                }
            });

            store = new Store<ITestPerson>(initial, reducer);

            // should throw.
            Assert.Throws<DispatchException>(() => store.Dispatch("Subscribe"));
        }

        /// <summary>
        /// Tests that getting the state during dispatch throws.
        /// </summary>
        [Fact]
        public void DispatchGetStateThrowsTest()
        {
            TestPerson initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
            };

            Store<ITestPerson>? store = null;

            var reducer = ITestPerson.Producer<object>((person, action) =>
            {
                switch (action)
                {
                    case "GetState":
                        Assert.NotSame(initial, store!.State);
                        break;
                }
            });

            store = new Store<ITestPerson>(initial, reducer);
            Assert.Throws<DispatchException>(() => store.Dispatch("GetState"));
        }

        /// <summary>
        /// Tests that dispatching during dispatch throws.
        /// </summary>
        [Fact]
        public void DispatchDispatchThrowsTest()
        {
            TestPerson initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
            };

            Store<ITestPerson>? store = null;

            var reducer = ITestPerson.Producer<object>((person, action) =>
            {
                switch (action)
                {
                    case "Dispatch":
                        store!.Dispatch("AnotherDispatch");
                        break;
                    case "AnotherDispatch":
                        throw new Exception("We should not have gotten here");
                }
            });

            store = new Store<ITestPerson>(initial, reducer);
            Assert.Throws<DispatchException>(() => store.Dispatch("Dispatch"));
        }
    }
}
