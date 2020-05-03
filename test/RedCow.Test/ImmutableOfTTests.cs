// <copyright file="ImmutableOfTTests.cs" company="Jan-Willem Spuij">
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
    using System.Text;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="Immutable{T}"/>.
    /// </summary>
    public class ImmutableOfTTests
    {
        /// <summary>
        /// Tests the Initial Produce Method.
        /// </summary>
        [Fact]
        public void InitialProduceTest()
        {
            var initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            };

            ITestPerson person = ITestPerson.Produce(initial);

            Assert.False(ReferenceEquals(initial, person));
            Assert.Equal(initial.FirstName, person.FirstName);
            Assert.Equal(initial.LastName, person.LastName);
            Assert.Equal(initial.IsAdult, person.IsAdult);
        }

        /// <summary>
        /// Tests the Produce Method.
        /// </summary>
        [Fact]
        public void StaticProduceTest()
        {
            TestPerson initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            };

            ITestPerson person = ITestPerson.Produce(initial, p =>
            {
                p.FirstName = "Jane";
                p.IsAdult = false;
            });

            Assert.False(ReferenceEquals(initial, person));
            Assert.NotEqual(initial.FirstName, person.FirstName);
            Assert.Equal(initial.LastName, person.LastName);
            Assert.NotEqual(initial.IsAdult, person.IsAdult);
        }

        /// <summary>
        /// Tests the Produce Method.
        /// </summary>
        [Fact]
        public void ProduceTest()
        {
            ITestPerson initial = ITestPerson.Produce(
                new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            });

            ITestPerson person = initial.Produce(p =>
            {
                p.FirstName = "Jane";
                p.IsAdult = false;
            });

            Assert.False(ReferenceEquals(initial, person));
            Assert.NotEqual(initial.FirstName, person.FirstName);
            Assert.Equal(initial.LastName, person.LastName);
            Assert.NotEqual(initial.IsAdult, person.IsAdult);
        }
    }
}
