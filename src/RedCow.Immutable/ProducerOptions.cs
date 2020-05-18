// <copyright file="ProducerOptions.cs" company="Jan-Willem Spuij">
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

namespace RedCow
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json.Serialization;
    using RedCow.Immutable;

    /// <summary>
    /// The producer options to use.
    /// </summary>
    public class ProducerOptions : IProducerOptions
    {
        /// <summary>
        /// Gets the default Producer options.
        /// </summary>
        public static ProducerOptions Default { get; } = new ProducerOptions();

        /// <summary>
        /// Gets the set of allowed immutable reference types.
        /// </summary>
        public ISet<Type> AllowedImmutableReferenceTypes { get; private set; } = new HashSet<Type>()
        {
            typeof(string),
            typeof(Type),
        };

        /// <summary>
        /// Gets the Patches document to fill.
        /// </summary>
        public JsonPatchDocument? Patches { get; private set; }

        /// <summary>
        /// Gets the inverse Patches document to fill.
        /// </summary>
        public JsonPatchDocument? InversePatches { get; private set; }

        /// <summary>
        /// Returns a new <see cref="ProducerOptions"/> instance with the specified options.
        /// </summary>
        /// <param name="patches">The patch docoment to store patches into.</param>
        /// <param name="inversePatches">The patch docoment to store inverse patches into.</param>
        /// <returns>A new <see cref="ProducerOptions"/> instance.</returns>
        public ProducerOptions WithPatches(JsonPatchDocument patches, JsonPatchDocument inversePatches)
        {
            if (patches is null)
            {
                throw new ArgumentNullException(nameof(patches));
            }

            if (inversePatches is null)
            {
                throw new ArgumentNullException(nameof(inversePatches));
            }

            return new ProducerOptions()
            {
                AllowedImmutableReferenceTypes = this.AllowedImmutableReferenceTypes,
                Patches = patches,
                InversePatches = inversePatches,
            };
        }
    }
}
