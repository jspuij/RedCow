// <copyright file="ITestPerson.cs" company="Jan-Willem Spuij">
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
    using System.Collections.Generic;

    /// <summary>
    /// Interface for an Immutable Test Person.
    /// </summary>
    [GenerateProducers(typeof(TestPerson))]
    public partial interface ITestPerson
    {
        /// <summary>
        /// Gets the first name.
        /// </summary>
        string FirstName { get; }

        /// <summary>
        /// Gets the last name.
        /// </summary>
        public string LastName { get; }

        /// <summary>
        /// Gets a value indicating whether isAdult is true.
        /// </summary>
        public bool IsAdult { get; }

        /// <summary>
        /// Gets the First Child.
        /// </summary>
        public ITestPerson FirstChild { get; }

        /// <summary>
        /// Gets the Second Schild.
        /// </summary>
        public ITestPerson SecondChild { get; }

        /// <summary>
        /// Gets the cars.
        /// </summary>
        public IReadOnlyList<ICar> Cars { get; }
    }
}
