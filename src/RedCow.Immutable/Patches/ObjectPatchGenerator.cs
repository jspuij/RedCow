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
    using System.IO;
    using System.Text;
    using Microsoft.AspNetCore.JsonPatch;
    using RedCow.Patches;

    /// <summary>
    /// Generates JSON Patches and inverse Patches for the differences between two objects.
    /// </summary>
    public class ObjectPatchGenerator : PatchGeneratorBase, IPatchGenerator
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
            basePath = CheckArgumentsAndNormalizePath(draft, basePath, patches, inversePatches);

            // nothing to do.
            if (!draft.DraftState!.Changed)
            {
                return;
            }

            object source = draft.DraftState.GetOriginal<object>();

            if (source is null)
            {
                throw new PatchGenerationException(draft, "The draft has no original state.");
            }

            if (!(draft is IPropertyAccessors draftProperties))
            {
                throw new PatchGenerationException(draft, "The draft has no propertyinfo.");
            }

            if (!(source is IPropertyAccessors sourceProperties))
            {
                throw new PatchGenerationException(draft, "The source has no propertyinfo.");
            }

            foreach (string propertyName in draftProperties.PublicPropertyGetters.Keys)
            {
                object? oldValue = sourceProperties.PublicPropertyGetters[propertyName]();
                object? newValue = draftProperties.PublicPropertyGetters[propertyName]();

                // if the newValue is a draft and points to the same original, it's actually the same object in
                // a mutable sense and the patches will reflect that. Otherwise we would end up with very large patches
                // of the entire tree.
                if (Equals(oldValue, newValue) ||
                    (newValue is IDraft newDraft && Equals(oldValue, newDraft.DraftState!.GetOriginal<object>())))
                {
                    continue;
                }
                else if (oldValue == null && newValue != null)
                {
                    patches.Add(basePath.PathJoin(propertyName), newValue);
                    inversePatches.Remove(basePath.PathJoin(propertyName));
                }
                else if (oldValue != null && newValue == null)
                {
                    patches.Remove(basePath.PathJoin(propertyName));
                    inversePatches.Add(basePath.PathJoin(propertyName), oldValue);
                }
                else
                {
                    patches.Replace(basePath.PathJoin(propertyName), newValue);
                    inversePatches.Replace(basePath.PathJoin(propertyName), oldValue);
                }
            }
        }
    }
}
