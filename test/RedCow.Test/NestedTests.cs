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

            // Copy on write.
            Assert.Same(initial, person);
            Assert.Same(initial.FirstChild, person.FirstChild);
            Assert.Same(initial.SecondChild, person.SecondChild);

            Assert.NotSame(person, result);
            Assert.NotSame(person.FirstChild, result.FirstChild);

            // this is the same immutable as it did not change.
            Assert.Same(person.SecondChild, result.SecondChild);
        }

        /// <summary>
        /// Tests a nested produce where an inner draft is changed.
        /// This should also change all parent drafts up to the root.
        /// </summary>
        [Fact]
        public void InnerNestedProduceTest()
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
                    FirstChild = new TestPerson()
                    {
                        FirstName = "Mika",
                        LastName = "Doe",
                        IsAdult = false,
                        FirstChild = new TestPerson()
                        {
                            FirstName = "Play",
                            LastName = "Doe",
                            IsAdult = false,
                        },
                    },
                },
            };

            ITestPerson person = ITestPerson.Produce(initial);

            var result = person.Produce(p =>
            {
                p.FirstChild.FirstChild.LastName = "Anon";
            });

            // Copy on write.
            Assert.Same(initial, person);
            Assert.Same(initial.FirstChild, person.FirstChild);
            Assert.Same(initial.FirstChild.FirstChild, person.FirstChild.FirstChild);
            Assert.Same(initial.FirstChild.FirstChild.FirstChild, person.FirstChild.FirstChild.FirstChild);

            Assert.NotSame(person, result);
            Assert.NotSame(person.FirstChild, result.FirstChild);
            Assert.NotSame(person.FirstChild.FirstChild, result.FirstChild.FirstChild);

            // this is the same immutable as it did not change.
            Assert.Same(person.FirstChild.FirstChild.FirstChild, result.FirstChild.FirstChild.FirstChild);
        }

        /// <summary>
        /// Tests for CoW of circular references.
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

            ITestPerson person = ITestPerson.Produce(initial);

            var result = person.Produce(p =>
            {
                p.LastName = "Bravo";
            });

            Assert.NotSame(initial, result);
            Assert.Equal("Bravo", result.LastName);
            Assert.NotSame(result, result.FirstChild);
            Assert.Equal("Doe", result.FirstChild.LastName);
        }

        /// <summary>
        /// Tests for CoW of nested circular references.
        /// </summary>
        [Fact]
        public void NestedCircularReferenceTest()
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
            };

            ITestPerson person = ITestPerson.Produce(initial);

            var result = person.Produce(p =>
            {
                p.FirstChild.FirstChild = p;
            });

            result = result.Produce(p =>
            {
                p.LastName = "Bravo";
            });

            Assert.NotSame(initial, result);
            Assert.Equal("Bravo", result.LastName);
            Assert.NotSame(result, result.FirstChild);
            Assert.Equal("Doe", result.FirstChild.FirstChild.LastName);
        }

        /// <summary>
        /// Tests for max level of nested circular references.
        /// </summary>
        [Fact]
        public void CircularReferenceExceedsMaxLevelTest()
        {
            var initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            };

            ITestPerson person = ITestPerson.Produce(initial);

            Assert.Throws<CircularReferenceException>(() =>
            {
                var result = person.Produce(p =>
                {
                    var person = p;
                    for (int i = 0; i < ProducerOptions.Default.MaxDepth + 1; i++)
                    {
                        person.FirstChild = new TestPerson()
                        {
                            FirstName = $"John {i}",
                            LastName = "Doe",
                            IsAdult = true,
                        };
                        person = person.FirstChild;
                    }
                });
            });
        }

        /// <summary>
        /// Tests the that the produced immutable cannot be altered.
        /// </summary>
        [Fact]
        public void NestedLockedTest()
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
                    FirstChild = new TestPerson()
                    {
                        FirstName = "Mika",
                        LastName = "Doe",
                        IsAdult = false,
                        FirstChild = new TestPerson()
                        {
                            FirstName = "Play",
                            LastName = "Doe",
                            IsAdult = false,
                        },
                    },
                },
            };

            ITestPerson person = ITestPerson.Produce(initial);

            var mutablePerson = (TestPerson)person;

            Assert.Throws<ImmutableException>(() =>
            {
                mutablePerson.FirstName = "Test";
            });
            Assert.Throws<ImmutableException>(() =>
            {
                mutablePerson.FirstChild.FirstName = "Test";
            });
            Assert.Throws<ImmutableException>(() =>
            {
                mutablePerson.FirstChild.FirstChild.FirstName = "Test";
            });
            Assert.Throws<ImmutableException>(() =>
            {
                mutablePerson.FirstChild.FirstChild.FirstChild.FirstName = "Test";
            });
        }
    }
}
