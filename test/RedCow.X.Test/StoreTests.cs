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

            bool observed = false;

            var observer = new DelegateObserver<ITestPerson>(value =>
            {
                observed = true;
                Assert.NotSame(initial, value);
                Assert.Equal("Jane", value.FirstName);
            });

            using var disposable = store.Subscribe(observer);
            store.Dispatch("ChangeGender");

            Assert.True(observed);
        }

        /// <summary>
        /// Tests that unsubscribing during dispatch works.
        /// </summary>
        [Fact]
        public void UnsubscribeDuringDispatch()
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

            IDisposable? disposable = null;

            var observer = new DelegateObserver<ITestPerson>(value =>
            {
                disposable!.Dispose();
            });

            disposable = store.Subscribe(observer);

            // should not throw.
            store.Dispatch("ChangeGender");
        }
    }
}
