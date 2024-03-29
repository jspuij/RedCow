﻿// <copyright file="JsonAssert.cs" company="Jan-Willem Spuij">
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
    using Newtonsoft.Json.Linq;
    using Xunit;

    /// <summary>
    /// Asserts for JSon objects.
    /// </summary>
    public static class JsonAssert
    {
        /// <summary>
        /// Asserts that two JSon strings are equal.
        /// </summary>
        /// <param name="expected">The expected JSON.</param>
        /// <param name="actual">The actual JSon.</param>
        public static void Equal(string expected, string actual)
        {
            var expectedJObject = JToken.Parse(expected);
            var actualJObject = JToken.Parse(actual);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), $"JSON expected: {expected} != actual: {actual}");
        }
    }
}
