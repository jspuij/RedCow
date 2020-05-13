// <copyright file="CollectionTests.cs" company="Jan-Willem Spuij">
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
    using System.Linq;
    using System.Text;
    using RedCow.Immutable;
    using Xunit;

    /// <summary>
    /// Unit tests for collections.
    /// </summary>
    public class CollectionTests
    {
        /// <summary>
        /// Tests producing a collection.
        /// </summary>
        [Fact]
        public void CollectionProduceTest()
        {
            var initial = new TestPerson()
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
            };

            var person = ITestPerson.Produce(initial);

            var result = person.Produce(p =>
            {
                p.Cars.Add(new Car()
                {
                    Make = "Rolls Royce",
                    Model = "10 HP",
                });
            });

            Assert.NotSame(initial, result);
            Assert.NotSame(initial.Cars, result.Cars);
            Assert.True(((ILockable)result.Cars.Last()).Locked);
        }

        /// <summary>
        /// Tests producing a collection.
        /// </summary>
        [Fact]
        public void CollectionNestedChangeTest()
        {
            var initial = new TestPerson()
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
            };

            var person = ITestPerson.Produce(initial);

            var result = person.Produce(p =>
            {
                p.Cars[1].Make = "Shelby American";
            });

            Assert.NotSame(initial, result);
            Assert.NotSame(initial.Cars, result.Cars);
            Assert.Same(initial.Cars[0], result.Cars[0]);
            Assert.NotSame(initial.Cars[1], result.Cars[1]);
        }

        /// <summary>
        /// Tests whether enumerating the collection, but not changing anything,
        /// returns the original collection.
        /// </summary>
        [Fact]
        public void UnchangedCollectionTest()
        {
            var initial = new TestPerson()
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
            };

            var person = ITestPerson.Produce(initial);

            var result = person.Produce(p =>
            {
                foreach (var car in p.Cars)
                {
                    Assert.True(car.IsDraft());
                }
            });

            Assert.Same(person.Cars, result.Cars);
        }

        /// <summary>
        /// Tests whether the collection is revoked.
        /// </summary>
        [Fact]
        public void CollectionRevokedTest()
        {
            var initial = new TestPerson()
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
            };

            var person = ITestPerson.Produce(initial);

            IList<Car>? cars = null;

            var result = person.Produce(p =>
            {
                cars = p.Cars;
            });

            Assert.NotSame(cars, result.Cars);
            Assert.Throws<DraftRevokedException>(() =>
            {
                Assert.Same(result.Cars[0], cars ![0]);
            });
        }
    }
}
