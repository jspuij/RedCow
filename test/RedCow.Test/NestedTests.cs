// <copyright file="NestedTests.cs" company="Jan-Willem Spuij">
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

namespace RedCow.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Xunit;

    /// <summary>
    /// Unit tests for nested produce and immutability.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NestedTests
    {
        /// <summary>
        /// Tests a nested produce.
        /// </summary>
        [Fact]
        public void NestedProduceTest()
        {
            var initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
                FirstChild = new TestPerson()
                {
                    FirstName = "Baby",
                    LastName = "Doe",
                    IsAdult = false,
                },
                SecondChild = new TestPerson()
                {
                    FirstName = "Mika",
                    LastName = "Doe",
                    IsAdult = false,
                },
            };

            ITestPerson person = ITestPerson.Produce(initial);

            var result = person.Produce(p =>
            {
                p.LastName = "Anon";
                p.FirstChild.LastName = "Anon";
            });

            Assert.NotSame(initial, person);
            Assert.NotSame(person, result);
            Assert.NotSame(person.FirstChild, result.FirstChild);

            // this is the same immutable as it did not change.
            Assert.Same(person.SecondChild, result.SecondChild);
        }

        /// <summary>
        /// Tests a nested produce.
        /// </summary>
        [Fact]
        public void NestedProduceChangedTest()
        {
            var initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
                FirstChild = new TestPerson()
                {
                    FirstName = "Baby",
                    LastName = "Doe",
                    IsAdult = false,
                },
                SecondChild = new TestPerson()
                {
                    FirstName = "Mika",
                    LastName = "Doe",
                    IsAdult = false,
                },
            };

            ITestPerson person = ITestPerson.Produce(initial);

            var result = person.Produce(p =>
            {
                p.LastName = "Anon";
                p.FirstChild.LastName = "Anon";
                var local = p.SecondChild;
                Assert.NotSame(local, initial.SecondChild);
            });

            Assert.NotSame(initial, person);
            Assert.NotSame(person, result);
            Assert.NotSame(person.FirstChild, result.FirstChild);

            // this is the same immutable as it did not change.
            Assert.Same(person.SecondChild, result.SecondChild);
        }

        /// <summary>
        /// Tests for circular references.
        /// </summary>
        [Fact]
        public void CircularReferenceTest()
        {
            var initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            };

            // this is really weird.
            initial.FirstChild = initial;

            Assert.Throws<CircularReferenceException>(() =>
            {
                ITestPerson person = ITestPerson.Produce(initial);
            });
        }
    }
}
