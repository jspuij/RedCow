// <copyright file="DictionaryPatchGeneratorTests.cs" company="Jan-Willem Spuij">
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
    using RedCow.Patches;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="DictionaryPatchGenerator"/> class.
    /// </summary>
    public class DictionaryPatchGeneratorTests
    {
        /// <summary>
        /// Tests the generation of an object patch.
        /// </summary>
        [Fact]
        public void GenerateDictionaryPatch()
        {
            var initial = new PhoneBook()
            {
                Entries = new Dictionary<string, TestPerson>()
                {
                    ["0800JOHNDOE"] = new TestPerson()
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        IsAdult = true,
                    },
                    ["0800JANEDOE"] = new TestPerson()
                    {
                        FirstName = "Jane",
                        LastName = "Doe",
                        IsAdult = true,
                    },
                },
            };

            using var scope = DraftExtensions.CreateDraft(initial, out PhoneBook draft);

            var patchGenerator = new DictionaryPatchGenerator();
            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            draft.Entries.Remove("0800JANEDOE");
            draft.Entries.Add("0800BABYDOE", new TestPerson()
            {
                FirstName = "Baby",
                LastName = "Doe",
            });

            patchGenerator.Generate((IDraft)draft.Entries, "/Entries", patches, inversePatches);

            // inverse order of inverse patches.
            inversePatches.Operations.Reverse();

            JsonAssert.Equal(
            @"
            [
              {
                'path': '/Entries/0800JANEDOE',
                'op': 'remove'
              },
              {
                'value': {
                  'Cars': null,
                  'FirstName': 'Baby',
                  'LastName': 'Doe',
                  'IsAdult': false,
                  'FirstChild': null,
                  'SecondChild': null
                },
                'path': '/Entries/0800BABYDOE',
                'op': 'add'
              }
            ]
            ", JsonConvert.SerializeObject(patches));

            JsonAssert.Equal(
            @"
            [
              {
                'path': '/Entries/0800BABYDOE',
                'op': 'remove'
              },
              {
                'value': {
                  'FirstName': 'Jane',
                  'LastName': 'Doe',
                  'IsAdult': true,
                  'FirstChild': null,
                  'SecondChild': null,
                  'Cars': null
                },
                'path': '/Entries/0800JANEDOE',
                'op': 'add'
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
            var initial = new PhoneBook()
            {
                Entries = new Dictionary<string, TestPerson>()
                {
                    ["0800JOHNDOE"] = new TestPerson()
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        IsAdult = true,
                    },
                    ["0800JANEDOE"] = new TestPerson()
                    {
                        FirstName = "Jane",
                        LastName = "Doe",
                        IsAdult = true,
                    },
                },
            };

            using DraftScope scope = (DraftScope)DraftExtensions.CreateDraft(initial, out PhoneBook draft);

            var patchGenerator = new DictionaryPatchGenerator();
            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            draft.Entries.Remove("0800JANEDOE");
            draft.Entries.Add("0800BABYDOE", new TestPerson()
            {
                FirstName = "Baby",
                LastName = "Doe",
            });

            // trick the scope into thinking that is finishing and should not create proxies anymore.
            scope.IsFinishing = true;

            patchGenerator.Generate((IDraft)draft.Entries, "/Entries", patches, inversePatches);

            // inverse order of inverse patches.
            inversePatches.Operations.Reverse();

            var final = scope.FinishDraft<PhoneBook, IPhoneBook>(draft);

            var result = IPhoneBook.Produce(initial, p =>
            {
                patches.ApplyTo(p);
            });

            Assert.Equal(2, result.Entries.Count);
            Assert.Same(initial.Entries["0800JOHNDOE"], result.Entries["0800JOHNDOE"]);
            Assert.NotSame(initial.Entries["0800JANEDOE"], result.Entries["0800BABYDOE"]);
        }

        /// <summary>
        /// Tests the generation and application of an object patch.
        /// </summary>
        [Fact]
        public void GenerateAndApplyReversePatch()
        {
            var initial = new PhoneBook()
            {
                Entries = new Dictionary<string, TestPerson>()
                {
                    ["0800JOHNDOE"] = new TestPerson()
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        IsAdult = true,
                    },
                    ["0800JANEDOE"] = new TestPerson()
                    {
                        FirstName = "Jane",
                        LastName = "Doe",
                        IsAdult = true,
                    },
                },
            };

            using DraftScope scope = (DraftScope)DraftExtensions.CreateDraft(initial, out PhoneBook draft);

            var patchGenerator = new DictionaryPatchGenerator();
            var patches = new JsonPatchDocument();
            var inversePatches = new JsonPatchDocument();

            draft.Entries.Remove("0800JANEDOE");
            draft.Entries.Add("0800BABYDOE", new TestPerson()
            {
                FirstName = "Baby",
                LastName = "Doe",
            });

            // trick the scope into thinking that is finishing and should not create proxies anymore.
            scope.IsFinishing = true;

            patchGenerator.Generate((IDraft)draft.Entries, "/Entries", patches, inversePatches);

            // inverse order of inverse patches.
            inversePatches.Operations.Reverse();

            var final = scope.FinishDraft<PhoneBook, IPhoneBook>(draft);

            var result = IPhoneBook.Produce(initial, p =>
            {
                patches.ApplyTo(p);
            });

            result = result.Produce(p =>
            {
                inversePatches.ApplyTo(p);
            });

            Assert.Equal(2, result.Entries.Count);
            Assert.Same(initial.Entries["0800JOHNDOE"], result.Entries["0800JOHNDOE"]);
            Assert.Same(initial.Entries["0800JANEDOE"], result.Entries["0800JANEDOE"]);
        }
    }
}
