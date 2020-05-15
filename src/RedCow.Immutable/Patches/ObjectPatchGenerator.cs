// <copyright file="ObjectPatchGenerator.cs" company="Jan-Willem Spuij">
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

namespace RedCow.Immutable.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.JsonPatch;
    using RedCow.Patches;

    /// <summary>
    /// Generates JSON Patches and inverse Patches for the differences between two objects.
    /// </summary>
    public class ObjectPatchGenerator : IPatchGenerator
    {
        /// <summary>
        /// Generates JSON Patches for a draft for changes and inverse changes and
        /// adds them to the specified JsonPatchDocuments.
        /// </summary>
        /// <param name="draft">The draft to process.</param>
        /// <param name="basePath">The base path for the patches.</param>
        /// <param name="patches">The patches.</param>
        /// <param name="inversePatches">The inverse patches.</param>
        public void Generate(IDraft draft, string? basePath, JsonPatchDocument patches, JsonPatchDocument inversePatches)
        {
            if (draft is null)
            {
                throw new ArgumentNullException(nameof(draft));
            }

            if (basePath is null)
            {
                basePath = string.Empty;
            }

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

            object destination = draft;
            object source = draft.DraftState.GetOriginal<object>();
        }
    }
}
