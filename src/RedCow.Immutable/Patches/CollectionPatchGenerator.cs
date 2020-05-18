// <copyright file="CollectionPatchGenerator.cs" company="Jan-Willem Spuij">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.JsonPatch;
    using RedCow.Immutable;
    using RedCow.Immutable.Patches;

    /// <summary>
    /// Generates patches for a collection.
    /// </summary>
    public class CollectionPatchGenerator : PatchGeneratorBase, IPatchGenerator
    {
        /// <summary>
        /// Provides the Longest Common Subsequence.
        /// </summary>
        private readonly ILongestCommonSubsequence longestCommonSubsequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionPatchGenerator"/> class.
        /// </summary>
        /// <param name="longestCommonSubsequence">A provider that will provide the Longest Common Subsequence of two sequences.</param>
        public CollectionPatchGenerator(ILongestCommonSubsequence longestCommonSubsequence)
        {
            this.longestCommonSubsequence = longestCommonSubsequence ?? throw new ArgumentNullException(nameof(longestCommonSubsequence));
        }

        /// <summary>
        /// Generates JSON Patches for a draft for changes and inverse changes and
        /// adds them to the specified JsonPatchDocuments.
        /// </summary>
        /// <param name="draft">The draft to process.</param>
        /// <param name="basePath">The base path for the patches.</param>
        /// <param name="patches">The patches.</param>
        /// <param name="inversePatches">The inverse patches.</param>
        public void Generate(IDraft draft, string basePath, JsonPatchDocument patches, JsonPatchDocument inversePatches)
        {
            basePath = CheckArgumentsAndNormalizePath(draft, basePath, patches, inversePatches);

            // nothing to do.
            if (!draft.DraftState!.Changed)
            {
                return;
            }

            IList draftList = (IList)draft;
            IList sourceList = draft.DraftState.GetOriginal<IList>();

            if (sourceList is null)
            {
                throw new PatchGenerationException(draft, "The draft has no original list state.");
            }

            int commonHead = 0;
            int commonTail = 0;

            // Equals function that compares both values, but also checks whether the newValue might be just a
            // draft of the old value.
            static bool DraftOrOriginalEquals(object oldValue, object newValue) =>
                Equals(oldValue, newValue) ||
                   (newValue is IDraft newDraft && Equals(oldValue, newDraft.DraftState!.GetOriginal<object>()));

            // Find common head
            while (commonHead < sourceList.Count
                && commonHead < draftList.Count
                && DraftOrOriginalEquals(sourceList[commonHead], draftList[commonHead]))
            {
                commonHead++;
            }

            // Find common tail
            while (commonTail + commonHead < sourceList.Count
                && commonTail + commonHead < draftList.Count
                && DraftOrOriginalEquals(sourceList[sourceList.Count - 1 - commonTail], draftList[draftList.Count - 1 - commonTail]))
            {
                commonTail++;
            }

            if (commonHead + commonTail == draftList.Count)
            {
                // Trivial case, a block (one or more consecutive items) was removed
                // reverse the order so that there is never a problem with different implementations
                // (patch atomicity v.s. operation atomicity).
                for (int index = sourceList.Count - commonTail - 1; index >= commonHead; --index)
                {
                    patches.Remove(basePath.PathJoin($"{index}"));
                    inversePatches.Add(basePath.PathJoin($"{(index < draftList.Count ? index.ToString() : "-")}"), sourceList[index]);
                }

                return;
            }

            if (commonHead + commonTail == sourceList.Count)
            {
                // Trivial case, a block (one or more consecutive items) was added
                for (int index = commonHead; index < draftList.Count - commonTail; ++index)
                {
                    patches.Add(basePath.PathJoin($"{(index < sourceList.Count ? index.ToString() : "-")}"), draftList[index]);
                    inversePatches.Remove(basePath.PathJoin($"{index}"));
                }

                return;
            }

            // complex case, use lcs to determine list operations.
            var lcs = this.longestCommonSubsequence.Get(
                sourceList,
                draftList,
                commonHead,
                sourceList.Count - commonTail - commonHead,
                commonHead,
                draftList.Count - commonTail - commonHead,
                DraftOrOriginalEquals);

            int lcsIndex = lcs.Length - 1;

            // reverse the order so that there is never a problem with different implementations
            // (patch atomicity v.s. operation atomicity).
            for (int index = sourceList.Count - commonTail - 1; index >= commonHead; --index)
            {
                if (lcsIndex < 0 || !Equals(sourceList[index], lcs[lcsIndex]))
                {
                    patches.Remove(basePath.PathJoin($"{index}"));
                    inversePatches.Add(basePath.PathJoin($"{(index < draftList.Count ? index.ToString() : "-")}"), sourceList[index]);
                } 
                else
                {
                    --lcsIndex;
                }
            }

            lcsIndex = 0;

            for (int index = commonHead; index < draftList.Count - commonTail; ++index)
            {
                if (lcsIndex >= lcs.Length || !DraftOrOriginalEquals(lcs[lcsIndex], draftList[index]))
                {
                    patches.Add(basePath.PathJoin($"{(index < lcs.Length ? index.ToString() : "-")}"), draftList[index]);
                    inversePatches.Remove(basePath.PathJoin($"{index}"));
                } 
                else
                {
                    ++lcsIndex;
                }
            }
        }
    }
}
