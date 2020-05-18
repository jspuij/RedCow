// <copyright file="CollectionPatchGeneratorTests.cs" company="Jan-Willem Spuij">
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

namespace RedCow.Test.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json;
    using RedCow.Immutable;
    using RedCow.Immutable.Patches;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="CollectionPatchGenerator"/> class.
    /// </summary>
    public class CollectionPatchGeneratorTests
    {
        /// <summary>
        /// Tests the generation of a trivial addition patch.
        /// </summary>
        [Fact]
        public void GenerateTrivialCollectionAdditionPatch()
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

            using var scope = DraftExtensions.CreateDraft(initial, out TestPerson draft);

            draft.Cars.Add(new Car()
            {
                Make = "Rolls Royce",
                Model = "10 HP",
            });
            draft.Cars.Add(new Car()
            {
                Make = "Mercedes-Benz",
                Model = "38/250 SSK",
            });

            var patchGenerator = new CollectionPatchGenerator(new DynamicLargestCommonSubsequence());
            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            patchGenerator.Generate((IDraft)draft.Cars, "/Cars", patches, inversePatches);

            // inverse order of inverse patches.
            inversePatches.Operations.Reverse();

            JsonAssert.Equal(
            @"
            [
              {
                'value': {
                  'Make': 'Rolls Royce',
                  'Model': '10 HP',
                  'Crashed': false
                },
                'path': '/Cars/-',
                'op': 'add'
              },
              {
                'value': {
                  'Make': 'Mercedes-Benz',
                  'Model': '38/250 SSK',
                  'Crashed': false
                },
                'path': '/Cars/-',
                'op': 'add'
              }
            ]
            ", JsonConvert.SerializeObject(patches));

            JsonAssert.Equal(
            @"
            [
              {
                'path': '/Cars/3',
                'op': 'remove'
              },
              {
                'path': '/Cars/2',
                'op': 'remove'
              }
            ]
            ", JsonConvert.SerializeObject(inversePatches));
        }

        /// <summary>
        /// Tests the generation of a trivial addition patch.
        /// </summary>
        [Fact]
        public void GenerateTrivialCollectionInsertionPatch()
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

            using var scope = DraftExtensions.CreateDraft(initial, out TestPerson draft);

            draft.Cars.Insert(0, new Car()
            {
                Make = "Rolls Royce",
                Model = "10 HP",
            });
            draft.Cars.Insert(1, new Car()
            {
                Make = "Mercedes-Benz",
                Model = "38/250 SSK",
            });

            var patchGenerator = new CollectionPatchGenerator(new DynamicLargestCommonSubsequence());
            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            patchGenerator.Generate((IDraft)draft.Cars, "/Cars", patches, inversePatches);

            // inverse order of inverse patches.
            inversePatches.Operations.Reverse();

            JsonAssert.Equal(
            @"
            [
              {
                'value': {
                  'Make': 'Rolls Royce',
                  'Model': '10 HP',
                  'Crashed': false
                },
                'path': '/Cars/0',
                'op': 'add'
              },
              {
                'value': {
                  'Make': 'Mercedes-Benz',
                  'Model': '38/250 SSK',
                  'Crashed': false
                },
                'path': '/Cars/1',
                'op': 'add'
              }
            ]
            ", JsonConvert.SerializeObject(patches));

            JsonAssert.Equal(
            @"
            [
              {
                'path': '/Cars/1',
                'op': 'remove'
              },
              {
                'path': '/Cars/0',
                'op': 'remove'
              }
            ]
            ", JsonConvert.SerializeObject(inversePatches));
        }

        /// <summary>
        /// Tests the generation of a trivial removal patch.
        /// </summary>
        [Fact]
        public void GenerateTrivialCollectionRemovalPatch()
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
                    new Car()
                    {
                        Make = "Rolls Royce",
                        Model = "10 HP",
                    },
                    new Car()
                    {
                        Make = "Mercedes-Benz",
                        Model = "38/250 SSK",
                    },
                    new Car()
                    {
                        Make = "Bugatti",
                        Model = "Type 57 SC Atalante",
                    },
                },
            };

            using var scope = DraftExtensions.CreateDraft(initial, out TestPerson draft);

            var patchGenerator = new CollectionPatchGenerator(new DynamicLargestCommonSubsequence());
            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            draft.Cars.RemoveAt(2);
            draft.Cars.RemoveAt(2);
            draft.Cars.RemoveAt(2);

            patchGenerator.Generate((IDraft)draft.Cars, "/Cars", patches, inversePatches);

            // inverse order of inverse patches.
            inversePatches.Operations.Reverse();

            JsonAssert.Equal(
            @"
            [
              {
                'path': '/Cars/4',
                'op': 'remove'
              },
              {
                'path': '/Cars/3',
                'op': 'remove'
              },
              {
                'path': '/Cars/2',
                'op': 'remove'
              }
            ]
            ", JsonConvert.SerializeObject(patches));

            JsonAssert.Equal(
            @"
            [
              {
                'value': {
                  'Make': 'Rolls Royce',
                  'Model': '10 HP',
                  'Crashed': false
                },
                'path': '/Cars/-',
                'op': 'add'
              },
              {
                'value': {
                  'Make': 'Mercedes-Benz',
                  'Model': '38/250 SSK',
                  'Crashed': false
                },
                'path': '/Cars/-',
                'op': 'add'
              },
              {
                'value': {
                  'Make': 'Bugatti',
                  'Model': 'Type 57 SC Atalante',
                  'Crashed': false
                },
                'path': '/Cars/-',
                'op': 'add'
              }
            ]
            ", JsonConvert.SerializeObject(inversePatches));
        }

        /// <summary>
        /// Tests the generation of a removal patch.
        /// </summary>
        [Fact]
        public void GenerateCollectionRemovalPatch()
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
                    new Car()
                    {
                        Make = "Rolls Royce",
                        Model = "10 HP",
                    },
                    new Car()
                    {
                        Make = "Mercedes-Benz",
                        Model = "38/250 SSK",
                    },
                },
            };

            using var scope = DraftExtensions.CreateDraft(initial, out TestPerson draft);

            var patchGenerator = new CollectionPatchGenerator(new DynamicLargestCommonSubsequence());
            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            draft.Cars.RemoveAt(3);
            draft.Cars.RemoveAt(0);
            draft.Cars.Add(new Car()
            {
                Make = "Bugatti",
                Model = "Type 57 SC Atalante",
            });

            patchGenerator.Generate((IDraft)draft.Cars, "/Cars", patches, inversePatches);

            // inverse order of inverse patches.
            inversePatches.Operations.Reverse();

            JsonAssert.Equal(
            @"
            [
              {
                'path': '/Cars/3',
                'op': 'remove'
              },
              {
                'path': '/Cars/0',
                'op': 'remove'
              },
              {
                'value': {
                  'Make': 'Bugatti',
                  'Model': 'Type 57 SC Atalante',
                  'Crashed': false
                },
                'path': '/Cars/-',
                'op': 'add'
              }
            ]
            ", JsonConvert.SerializeObject(patches));

            JsonAssert.Equal(
            @"
            [
              {
                'path': '/Cars/2',
                'op': 'remove'
              },
              {
                'value': {
                  'Make': 'Ferrari',
                  'Model': '250 LM',
                  'Crashed': false
                },
                'path': '/Cars/0',
                'op': 'add'
              },
              {
                'value': {
                  'Make': 'Mercedes-Benz',
                  'Model': '38/250 SSK',
                  'Crashed': false
                },
                'path': '/Cars/-',
                'op': 'add'
              }            ]
            ", JsonConvert.SerializeObject(inversePatches));
        }
    }
}
