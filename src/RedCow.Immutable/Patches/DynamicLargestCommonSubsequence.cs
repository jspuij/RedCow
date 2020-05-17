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
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Implementation of standard dynamic programming solution for the
    /// Largest Common Subsequence problem for two sequences.
    /// </summary>
    public class DynamicLargestCommonSubsequence : ILongestCommonSubsequence
    {
        /// <summary>
        /// Gets the longest common subsequence in the elements from two spans.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="left">The left span. </param>
        /// <param name="right">The right span.</param>
        /// <param name="equalityComparer">The equality comparison function.</param>
        /// <returns>The longest common subsequence.</returns>
        public IEnumerable<T> Get<T>(ReadOnlySpan<T> left, ReadOnlySpan<T> right, Func<T, T, bool> equalityComparer)
        {
            var matrix = CreateLcsMatrix(left, right, equalityComparer);
            return TraceBack(matrix, left, right, equalityComparer);
        }

        /// <summary>
        /// Trace back through the matrix to find the Longest Common Subsequence.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="matrix">The matrix.</param>
        /// <param name="left">The left span.</param>
        /// <param name="right">The right span.</param>
        /// <param name="equalityComparer">The equality comparison function.</param>
        /// <returns>The Longest Common Subsequence.</returns>
        private static IEnumerable<T> TraceBack<T>(int[,] matrix, ReadOnlySpan<T> left, ReadOnlySpan<T> right, Func<T, T, bool> equalityComparer)
        {
            var result = new T[matrix[left.Length, right.Length]];

            for (int i = left.Length, j = right.Length; i > 0 && j > 0;)
            {
                if (equalityComparer(left[i - 1], right[j - 1]))
                {
                    result[matrix[i, j] - 1] = left[i - 1];
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
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="left">The left span. </param>
        /// <param name="right">The right span.</param>
        /// <param name="equalityComparer">The equality comparison function.</param>
        /// <returns>The matrix with the sequence length at the index positions.</returns>
        private static int[,] CreateLcsMatrix<T>(ReadOnlySpan<T> left, ReadOnlySpan<T> right, Func<T, T, bool> equalityComparer)
        {
            // first row and column in the array all zeroes to make the lookups a bit easier and faster.
            var arr = new int[left.Length + 1, right.Length + 1];

            for (int i = 1; i <= left.Length; i++)
            {
                for (int j = 1; j <= right.Length; j++)
                {
                    if (equalityComparer(left[i - 1], right[j - 1]))
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
