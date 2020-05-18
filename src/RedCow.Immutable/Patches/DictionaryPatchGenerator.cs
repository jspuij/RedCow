// <copyright file="DictionaryPatchGenerator.cs" company="Jan-Willem Spuij">
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
    using System.Collections;
    using System.Text;
    using Microsoft.AspNetCore.JsonPatch;
    using RedCow.Immutable;
    using RedCow.Immutable.Patches;

    /// <summary>
    /// Generates patches for a dictionary.
    /// </summary>
    public class DictionaryPatchGenerator : PatchGeneratorBase, IPatchGenerator
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

            IDictionary draftDictionary = (IDictionary)draft;
            IDictionary sourceDictionary = draft.DraftState.GetOriginal<IDictionary>();

            if (sourceDictionary is null)
            {
                throw new PatchGenerationException(draft, "The draft has no original dictionary state.");
            }

            // Equals function that compares both values, but also checks whether the newValue might be just a
            // draft of the old value.
            static bool DraftOrOriginalEquals(object oldValue, object newValue) =>
                Equals(oldValue, newValue) ||
                   (newValue is IDraft newDraft && Equals(oldValue, newDraft.DraftState!.GetOriginal<object>()));

            foreach (DictionaryEntry entry in sourceDictionary)
            {
                string key = entry.Key.ToString();

                if (!draftDictionary.Contains(key))
                {
                    patches.Remove(basePath.PathJoin(key));
                    inversePatches.Add(basePath.PathJoin(key), sourceDictionary[entry.Key]);
                }
                else if (!DraftOrOriginalEquals(sourceDictionary[entry.Key], draftDictionary[entry.Key]))
                {
                    patches.Replace(basePath.PathJoin(key), entry.Value);
                    inversePatches.Replace(basePath.PathJoin(key), sourceDictionary[entry.Key]);
                }
            }

            foreach (DictionaryEntry entry in draftDictionary)
            {
                string key = entry.Key.ToString();

                if (!sourceDictionary.Contains(entry.Key))
                {
                    patches.Add(basePath.PathJoin(key), entry.Value);
                    inversePatches.Remove(basePath.PathJoin(key));
                }
            }
        }
    }
}
