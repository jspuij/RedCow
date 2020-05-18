// <copyright file="DynamicLargestCommonSubsequence.cs" company="Jan-Willem Spuij">
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
    using System.Text;

    /// <summary>
    /// Implementation of standard dynamic programming solution for the
    /// Largest Common Subsequence problem for two sequences.
    /// </summary>
    public class DynamicLargestCommonSubsequence : ILongestCommonSubsequence
    {
        /// <summary>
        /// Gets the longest common subsequence in the elements from two lists.
        /// </summary>
        /// <param name="left">The left span. </param>
        /// <param name="right">The right span.</param>
        /// <param name="leftStart">The start index for the left list.</param>
        /// <param name="leftLength">The length for the left list.</param>
        /// <param name="rightStart">The start index for the right list.</param>
        /// <param name="rightLength">The length for the right list. </param>
        /// <param name="equalityComparer">The equality comparison function.</param>
        /// <returns>The longest common subsequence.</returns>
        public object[] Get(IList left, IList right, int leftStart, int leftLength, int rightStart, int rightLength, Func<object, object, bool> equalityComparer)
        {
            var matrix = CreateLcsMatrix(left, right, leftStart, leftLength, rightStart, rightLength, equalityComparer);
            return TraceBack(matrix, left, right, leftStart, leftLength, rightStart, rightLength, equalityComparer);
        }

        /// <summary>
        /// Trace back through the matrix to find the Longest Common Subsequence.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="left">The left span. </param>
        /// <param name="right">The right span.</param>
        /// <param name="leftStart">The start index for the left list.</param>
        /// <param name="leftLength">The length for the left list.</param>
        /// <param name="rightStart">The start index for the right list.</param>
        /// <param name="rightLength">The length for the right list. </param>
        /// <param name="equalityComparer">The equality comparison function.</param>
        /// <returns>The Longest Common Subsequence.</returns>
        private static object[] TraceBack(int[,] matrix, IList left, IList right, int leftStart, int leftLength, int rightStart, int rightLength, Func<object, object, bool> equalityComparer)
        {
            var result = new object[matrix[leftLength, rightLength]];

            for (int i = leftLength, j = rightLength; i > 0 && j > 0;)
            {
                if (equalityComparer(left[leftStart + i - 1], right[rightStart + j - 1]))
                {
                    result[matrix[i, j] - 1] = left[leftStart + i - 1];
                    i--;
                    j--;
                    continue;
                }

                if (matrix[i, j - 1] > matrix[i - 1, j])
                {
                    j--;
                }
                else
                {
                    i--;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the Lcs Matrix to backtrack through.
        /// </summary>
        /// <param name="left">The left span. </param>
        /// <param name="right">The right span.</param>
        /// <param name="leftStart">The start index for the left list.</param>
        /// <param name="leftLength">The length for the left list.</param>
        /// <param name="rightStart">The start index for the right list.</param>
        /// <param name="rightLength">The length for the right list. </param>
        /// <param name="equalityComparer">The equality comparison function.</param>
        /// <returns>The matrix with the sequence length at the index positions.</returns>
        private static int[,] CreateLcsMatrix(IList left, IList right, int leftStart, int leftLength, int rightStart, int rightLength, Func<object, object, bool> equalityComparer)
        {
            // first row and column in the array all zeroes to make the lookups a bit easier and faster.
            var arr = new int[leftLength + 1, rightLength + 1];

            for (int i = 1; i <= leftLength; i++)
            {
                for (int j = 1; j <= rightLength; j++)
                {
                    if (equalityComparer(left[leftStart + i - 1], right[rightStart + j - 1]))
                    {
                        arr[i, j] = arr[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        arr[i, j] = Math.Max(arr[i - 1, j], arr[i, j - 1]);
                    }
                }
            }

            return arr;
        }
    }
}
