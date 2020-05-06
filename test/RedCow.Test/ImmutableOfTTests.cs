﻿// <copyright file="ImmutableOfTTests.cs" company="Jan-Willem Spuij">
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
    using System.Linq;
    using System.Text;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="Immutable{T}"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
        public void StaticProduceActionTest()
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
        public void ProduceActionTest()
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

        /// <summary>
        /// Tests the Produce Method.
        /// </summary>
        [Fact]
        public void StaticProduceFunctionTest()
        {
            TestPerson initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            };

            ITestPerson person = ITestPerson.Produce(initial, () =>
                new TestPerson()
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    IsAdult = false,
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
        public void ProduceFunctionTest()
        {
            ITestPerson initial = ITestPerson.Produce(
                new TestPerson()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    IsAdult = true,
                });

            ITestPerson person = initial.Produce(() =>
                new TestPerson()
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    IsAdult = false,
                });

            Assert.False(ReferenceEquals(initial, person));
            Assert.NotEqual(initial.FirstName, person.FirstName);
            Assert.Equal(initial.LastName, person.LastName);
            Assert.NotEqual(initial.IsAdult, person.IsAdult);
        }

        /// <summary>
        /// Tests the Producer Method.
        /// </summary>
        [Fact]
        public void ProducerActionTest()
        {
            var initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            };

            var producer = ITestPerson.Producer(p =>
            {
                p.FirstName = "Jane";
                p.IsAdult = false;
            });

            var person = producer(initial);

            Assert.False(ReferenceEquals(initial, person));
            Assert.NotEqual(initial.FirstName, person.FirstName);
            Assert.Equal(initial.LastName, person.LastName);
            Assert.NotEqual(initial.IsAdult, person.IsAdult);
        }

        /// <summary>
        /// Tests the Produce Method.
        /// </summary>
        [Fact]
        public void ProducerFunctionTest()
        {
            var initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            };

            var producer = ITestPerson.Producer(() =>
                new TestPerson()
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    IsAdult = false,
                });

            var person = producer(initial);

            Assert.False(ReferenceEquals(initial, person));
            Assert.NotEqual(initial.FirstName, person.FirstName);
            Assert.Equal(initial.LastName, person.LastName);
            Assert.NotEqual(initial.IsAdult, person.IsAdult);
        }

        /// <summary>
        /// Tests the Producer Method with argument.
        /// </summary>
        [Fact]
        public void ProducerActionWithArgumentTest()
        {
            var initial = new[]
            {
                new TestPerson
                {
                    FirstName = "John",
                    LastName = "Doe",
                    IsAdult = true,
                },
                new TestPerson
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    IsAdult = false,
                },
            };

            var anonimizer = ITestPerson.Producer<int>((p, i) =>
            {
                p.LastName = $"Anonimized nr. {i + 1}";
            });

            var result = initial.Select((p, i) => anonimizer(p, i));

            int index = 0;

            Assert.All(initial.Zip(result, (first, second) => new { Expected = first, Actual = second, Index = ++index }), r =>
            {
                Assert.False(ReferenceEquals(r.Expected, r.Actual));
                Assert.Equal(r.Expected.FirstName, r.Actual.FirstName);
                Assert.NotEqual(r.Expected.LastName, r.Actual.LastName);
                Assert.Equal(r.Expected.IsAdult, r.Actual.IsAdult);
                Assert.Equal($"Anonimized nr. {r.Index}", r.Actual.LastName);
            });
        }
    }
}
