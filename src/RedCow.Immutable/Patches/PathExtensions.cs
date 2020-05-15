// <copyright file="PathExtensions.cs" company="Jan-Willem Spuij">
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
    /// Extension methods for JSON Paths.
    /// </summary>
    public static class PathExtensions
    {
        /// <summary>
        /// Joins two paths together with the slash as separator.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="subPath">The sub path.</param>
        /// <returns>The joined path.</returns>
        public static string PathJoin(this string basePath, string subPath)
        {
            return $"{basePath.TrimEnd('/')}/{subPath.Trim('/')}";
        }
    }
}
