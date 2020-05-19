// <copyright file="PatchTests.cs" company="Jan-Willem Spuij">
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
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json;
    using RedCow.Test.Patches;
    using Xunit;

    /// <summary>
    /// Unit tests for patches.
    /// </summary>
    public class PatchTests
    {
        /// <summary>
        /// Test using producers with patches.
        /// </summary>
        [Fact]
        public void ProduceWithPatchesTest()
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

            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            var options = ProducerOptions.Default.WithPatches(patches, inversePatches);

            var result = initial.Produce(
                p =>
            {
                p.LastName = "SadDoe";
                p.Cars[0].Crashed = true;
            }, options);

            JsonAssert.Equal(
            @"
            [
              {
                'value': true,
                'path': '/Cars/0/Crashed',
                'op': 'replace'
              },
              {
                'value': 'SadDoe',
                'path': '/LastName',
                'op': 'replace'
              }
            ]
            ", JsonConvert.SerializeObject(patches));

            JsonAssert.Equal(
            @"
            [
              {
                'value': 'Doe',
                'path': '/LastName',
                'op': 'replace'
              },
              {
                'value': false,
                'path': '/Cars/0/Crashed',
                'op': 'replace'
              }
            ]
            ", JsonConvert.SerializeObject(inversePatches));
        }

        /// <summary>
        /// Test using nested producers with patches.
        /// </summary>
        [Fact]
        public void NestedProduceWithPatchesTest()
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

            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            var options = ProducerOptions.Default.WithPatches(patches, inversePatches);

            var crasher = ICar.Producer(c => c.Crashed = true);

            var result = initial.Produce(
                p =>
                {
                    p.LastName = "SadDoe";

                    for (int i = 0; i < p.Cars.Count; i++)
                    {
                        p.Cars[i] = (Car)crasher.Invoke(p.Cars[i]);
                    }
                }, options);

            JsonAssert.Equal(
            @"
           [
              {
                'value': true,
                'path': '/Cars/0/Crashed',
                'op': 'replace'
              },
              {
                'value': true,
                'path': '/Cars/1/Crashed',
                'op': 'replace'
              },
              {
                'value': 'SadDoe',
                'path': '/LastName',
                'op': 'replace'
              }
            ]
            ", JsonConvert.SerializeObject(patches));

            JsonAssert.Equal(
            @"
            [
              {
                'value': 'Doe',
                'path': '/LastName',
                'op': 'replace'
              },
              {
                'value': false,
                'path': '/Cars/1/Crashed',
                'op': 'replace'
              },
              {
                'value': false,
                'path': '/Cars/0/Crashed',
                'op': 'replace'
              }
            ]
            ", JsonConvert.SerializeObject(inversePatches));
        }
    }
}
