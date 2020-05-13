// <copyright file="ImmutableProducerTests.cs" company="Jan-Willem Spuij">
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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using RedCow.Immutable;
    using Xunit;

    /// <summary>
    /// Unit tests for Immutable Producers.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ImmutableProducerTests
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

            // Copy on write so the objects should be identical.
            Assert.Same(initial, person);
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

            Assert.NotSame(initial, person);
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

            Assert.NotSame(initial, person);
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

            Assert.NotSame(initial, person);
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

            Assert.NotSame(initial, person);
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

            Assert.NotSame(initial, person);
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

            Assert.NotSame(initial, person);
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
                Assert.NotSame(r.Expected, r.Actual);
                Assert.Equal(r.Expected.FirstName, r.Actual.FirstName);
                Assert.NotEqual(r.Expected.LastName, r.Actual.LastName);
                Assert.Equal(r.Expected.IsAdult, r.Actual.IsAdult);
                Assert.Equal($"Anonimized nr. {r.Index}", r.Actual.LastName);
            });
        }

        /// <summary>
        /// Tests the that the produced immutable cannot be altered.
        /// </summary>
        [Fact]
        public void LockedTest()
        {
            var initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            };

            ITestPerson person = ITestPerson.Produce(initial);

            Assert.Throws<ImmutableException>(() =>
            {
                var mutablePerson = (TestPerson)person;

                mutablePerson.FirstName = "Test";
            });
        }

        /// <summary>
        /// Tests that a draft is revoked after the immutable is produced.
        /// </summary>
        [Fact]
        public void DraftRevokedTest()
        {
            ITestPerson initial = ITestPerson.Produce(new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            });

            TestPerson? draft = null;

            ITestPerson person = initial.Produce(p =>
            {
                draft = p;
            });

            Assert.Throws<DraftRevokedException>(() =>
            {
                Assert.Equal(draft!.FirstName, person.FirstName);
            });
        }

        /// <summary>
        /// Tests that during produce new state is not drafted.
        /// </summary>
        [Fact]
        public void DontDraftNewState()
        {
            ITestPerson initial = ITestPerson.Produce(new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            });

            initial.Produce(p =>
            {
                var child = new TestPerson()
                {
                    FirstName = "Baby",
                    LastName = "Joe,",
                    IsAdult = false,
                };
                p.FirstChild = child;
                Assert.Same(child, p.FirstChild);
                Assert.False(p.FirstChild.IsDraft());
            });
        }
    }
}
