// <copyright file="PathSegment.cs" company="Jan-Willem Spuij">
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
    /// Represents a patch segment.
    /// </summary>
    public class PathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathSegment"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PathSegment(string value)
            : this(null, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathSegment"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="value">The value.</param>
        public PathSegment(PathSegment? parent, string value)
        {
            this.Parent = parent;
            this.Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        public PathSegment? Parent { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Returns the string that represents the entire path to the root.
        /// </summary>
        /// <returns>The path.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            void Recurse(PathSegment? segment)
            {
                if (segment == null)
                {
                    return;
                }

                Recurse(segment.Parent);
                builder.Append($"{segment.Value.Trim('/')}/");
            }

            Recurse(this);

            return builder.ToString().TrimEnd('/');
        }
    }
}
