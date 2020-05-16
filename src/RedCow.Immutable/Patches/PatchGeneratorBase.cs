// <copyright file="PatchGeneratorBase.cs" company="Jan-Willem Spuij">
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

namespace RedCow.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.JsonPatch;
    using RedCow.Immutable;
    using RedCow.Immutable.Patches;

    /// <summary>
    /// Base class for Patch Generation.
    /// </summary>
    public abstract class PatchGeneratorBase
    {
        /// <summary>
        /// Checks the arguments of the generator.
        /// </summary>
        /// <param name="draft">The draft.</param>
        /// <param name="basePath">The base path.</param>
        /// <param name="patches">The patches collection to add to.</param>
        /// <param name="inversePatches">The inverse patches to add to.</param>
        /// <returns>The adjusted base path.</returns>
        protected static string CheckArgumentsAndNormalizePath(IDraft draft, string? basePath, JsonPatchDocument patches, JsonPatchDocument inversePatches)
        {
            if (draft is null)
            {
                throw new ArgumentNullException(nameof(draft));
            }

            basePath = $"/{(basePath ?? string.Empty).Trim()}";

            if (patches is null)
            {
                throw new ArgumentNullException(nameof(patches));
            }

            if (inversePatches is null)
            {
                throw new ArgumentNullException(nameof(inversePatches));
            }

            if (draft.DraftState == null)
            {
                throw new PatchGenerationException(draft, "The draft has no draft state.");
            }

            return basePath;
        }
    }
}
