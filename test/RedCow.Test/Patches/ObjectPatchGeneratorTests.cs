// <copyright file="ObjectPatchGeneratorTests.cs" company="Jan-Willem Spuij">
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
    using Microsoft.AspNetCore.JsonPatch.Operations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RedCow.Immutable;
    using RedCow.Immutable.Patches;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="ObjectPatchGenerator"/> class.
    /// </summary>
    public class ObjectPatchGeneratorTests
    {
        /// <summary>
        /// Tests the generation of an object patch.
        /// </summary>
        [Fact]
        public void GenerateObjectPatch()
        {
            var initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            };

            using var scope = DraftExtensions.CreateDraft(initial, out TestPerson draft);

            draft.FirstName = "Jane";
            draft.LastName = null;
            draft.FirstChild = new TestPerson()
            {
                FirstName = "Baby",
                LastName = "Doe",
            };

            var patchGenerator = new ObjectPatchGenerator();
            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            patchGenerator.Generate((IDraft)draft, "/", patches, inversePatches);

            // inverse order of inverse patches.
            inversePatches.Operations.Reverse();

            JsonAssert.Equal(
            @"
            [
              {
                'value': 'Jane',
                'path': '/FirstName',
                'op': 'replace'
              },
              {
                'path': '/LastName',
                'op': 'remove'
              },
              {
                'value': {
                  'FirstName': 'Baby',
                  'LastName': 'Doe',
                  'IsAdult': false,
                  'FirstChild': null,
                  'SecondChild': null,
                  'Cars': null
                },
                'path': '/FirstChild',
                'op': 'add'
              }
            ]
            ", JsonConvert.SerializeObject(patches));

            JsonAssert.Equal(
            @"
            [
              {
                'path': '/FirstChild',
                'op': 'remove'
              },
              {
                'value': 'Doe',
                'path': '/LastName',
                'op': 'add'
              },
              {
                'value': 'John',
                'path': '/FirstName',
                'op': 'replace'
              }
            ]
            ", JsonConvert.SerializeObject(inversePatches));
        }

        /// <summary>
        /// Tests the generation and application of an object patch.
        /// </summary>
        [Fact]
        public void GenerateAndApplyPatch()
        {
            var initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            };

            using var scope = DraftExtensions.CreateDraft(initial, out TestPerson draft);

            draft.FirstName = "Jane";
            draft.LastName = null;
            draft.FirstChild = new TestPerson()
            {
                FirstName = "Baby",
                LastName = "Doe",
            };

            var patchGenerator = new ObjectPatchGenerator();
            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            patchGenerator.Generate((IDraft)draft, "/", patches, inversePatches);

            // inverse order of inverse patches.
            inversePatches.Operations.Reverse();

            var final = scope.FinishDraft<TestPerson, ITestPerson>(draft);

            var result = ITestPerson.Produce(initial, p =>
            {
                patches.ApplyTo(p);
            });

            Assert.Equal(final.FirstName, result.FirstName);
            Assert.Equal(final.LastName, result.LastName);
            Assert.Equal(final.FirstChild.FirstName, result.FirstChild.FirstName);
            Assert.Equal(final.FirstChild.LastName, result.FirstChild.LastName);
        }

        /// <summary>
        /// Tests the generation and application of an object patch.
        /// </summary>
        [Fact]
        public void GenerateAndApplyReversePatch()
        {
            var initial = new TestPerson()
            {
                FirstName = "John",
                LastName = "Doe",
                IsAdult = true,
            };

            using var scope = DraftExtensions.CreateDraft(initial, out TestPerson draft);

            draft.FirstName = "Jane";
            draft.LastName = null;
            draft.FirstChild = new TestPerson()
            {
                FirstName = "Baby",
                LastName = "Doe",
            };

            var patchGenerator = new ObjectPatchGenerator();
            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            patchGenerator.Generate((IDraft)draft, "/", patches, inversePatches);

            // inverse order of inverse patches.
            inversePatches.Operations.Reverse();

            var final = scope.FinishDraft<TestPerson, ITestPerson>(draft);

            var result = final.Produce(p =>
            {
                inversePatches.ApplyTo(p);
            });

            Assert.Equal(initial.FirstName, result.FirstName);
            Assert.Equal(initial.LastName, result.LastName);
            Assert.Equal(initial.FirstChild, result.FirstChild);
        }
    }
}
