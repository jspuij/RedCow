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
        /// Tests a nested change.
        /// </summary>
        [Fact]
        public void NestedChangeTest()
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
        /// Tests a nested change where an inner draft is changed.
        /// This should also change all parent drafts up to the root.
        /// </summary>
        [Fact]
        public void InnerNestedChangeTest()
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
        /// Tests for CoW of circular references with a pointer update to close the loop.
        /// </summary>
        [Fact]
        public void CircularReferenceWithPointerUpdateTest()
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
                p.FirstChild = p;
            });

            Assert.NotSame(initial, result);
            Assert.Equal("Bravo", result.LastName);
            Assert.Same(result, result.FirstChild);
            Assert.Equal("Bravo", result.FirstChild.LastName);
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

            // A circular reference "unrolls" by forming a tree, pointing to the
            // older version, whenever the draft containing the circular reference
            // pointer is updated.  The older version will keep the circular reference.
            Assert.NotSame(initial, result);
            Assert.Equal("Bravo", result.LastName);
            Assert.NotSame(result, result.FirstChild.FirstChild);
            Assert.Equal("Doe", result.FirstChild.FirstChild.LastName);
        }

        /// <summary>
        /// Tests for CoW of nested circular references with a pointer update to close the loop.
        /// </summary>
        [Fact]
        public void NestedCircularReferenceWithPointerUpdateTest()
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
                p.LastName = "Bravo";
                p.FirstChild.FirstChild = p;
            });

            // A circular reference "unrolls" by forming a tree, pointing to the
            // older version, whenever the draft containing the circular reference
            // pointer is updated.  The older version will keep the circular reference.
            Assert.NotSame(initial, result);
            Assert.Equal("Bravo", result.LastName);
            Assert.Same(result, result.FirstChild.FirstChild);
            Assert.Equal("Bravo", result.FirstChild.FirstChild.LastName);
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

        /// <summary>
        /// Tests that a draft is revoked after the immutable is produced.
        /// </summary>
        [Fact]
        public void NestedDraftRevokedTest()
        {
            ITestPerson initial = ITestPerson.Produce(new TestPerson()
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
            });

            TestPerson? draft = null;
            TestPerson? draftChild = null;

            ITestPerson person = initial.Produce(p =>
            {
                draft = p;
                draftChild = p.FirstChild;
            });

            Assert.Throws<DraftRevokedException>(() =>
            {
                Assert.Equal(draft!.FirstName, person.FirstName);
            });
            Assert.Throws<DraftRevokedException>(() =>
            {
                Assert.Equal(draftChild!.FirstName, person.FirstChild.FirstName);
            });
        }

        /// <summary>
        /// Test using nested producers.
        /// </summary>
        [Fact]
        public void NestedProduceTest()
        {
            ITestPerson initial = ITestPerson.Produce(new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
                Cars = new List<Car>()
                {
                    new Car
                    {
                        Make = "Ferrari",
                        Model = "250 LM",
                    },
                    new Car
                    {
                        Make = "Shelby",
                        Model = "Daytona Cobra Coupe",
                    },
                },
            });

            var crasher = ICar.Producer(car =>
            {
                car.Crashed = true;
            });

            var result = initial.Produce(p =>
            {
                p.LastName = "SadDoe";
                p.Cars[0] = (Car)crasher(p.Cars[0]);
            });

            Assert.NotSame(initial, result);
            Assert.Equal("SadDoe", result.LastName);
            Assert.True(result.Cars[0].Crashed);
        }

        /// <summary>
        /// Test using nested producers.
        /// </summary>
        [Fact]
        public void NestedProduceCanContinueTest()
        {
            ITestPerson initial = ITestPerson.Produce(new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
                Cars = new List<Car>()
                {
                    new Car
                    {
                        Make = "Ferrari",
                        Model = "250 LM",
                    },
                    new Car
                    {
                        Make = "Shelby",
                        Model = "Daytona Cobra Coupe",
                    },
                },
            });

            var crasher = ICar.Producer(car =>
            {
                car.Crashed = true;
            });

            var fixer = ICar.Producer(car =>
            {
                car.Crashed = false;
            });

            var result = initial.Produce(p =>
            {
                p.LastName = "SadDoe";
                var crashedCar = (Car)crasher(p.Cars[0]);
                p.Cars[0] = crashedCar;
                p.Cars[0].Make = "Enzo Ferrari";
                var fixedCar = (Car)fixer(p.Cars[0]);
                Assert.NotSame(crashedCar, fixedCar);
                p.Cars[0] = fixedCar;
            });

            Assert.NotSame(initial, result);
            Assert.Equal("SadDoe", result.LastName);
            Assert.False(result.Cars[0].Crashed);
            Assert.Equal("Enzo Ferrari", result.Cars[0].Make);
        }

        /// <summary>
        /// Test using nested producers with a rollback in the middle.
        /// </summary>
        [Fact]
        public void NestedProduceRollbackTest()
        {
            ITestPerson initial = ITestPerson.Produce(new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
                Cars = new List<Car>()
                {
                    new Car
                    {
                        Make = "Ferrari",
                        Model = "250 LM",
                    },
                    new Car
                    {
                        Make = "Shelby",
                        Model = "Daytona Cobra Coupe",
                    },
                },
            });

            Car? car = null;

            var result = initial.Produce(p =>
            {
                p.Cars[0].Make = "Ford";
                p.Cars[0].Model = "Fiesta";
                try
                {
                    p.Cars[0] = (Car)ICar.Produce(p.Cars[0], c =>
                    {
                        c.Make = "Tesla";
                        c.Model = "Model 3";
                        car = c;
                        throw new InvalidOperationException("Cars cannot be changed.");
                    });
                }
                catch (InvalidOperationException)
                {
                    // car is revoked as it was inside the nested produce.
                    Assert.Throws<DraftRevokedException>(() => car!.Make);
                }
            });

            Assert.NotSame(initial, result);

            // We rolled back the changes on the inner produce, but we did keep the drafts for the outer changes.
            Assert.Equal("Ford", result.Cars[0].Make);
            Assert.Equal("Fiesta", result.Cars[0].Model);
        }
    }
}
