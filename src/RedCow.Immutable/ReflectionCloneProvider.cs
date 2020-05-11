// <copyright file="ReflectionCloneProvider.cs" company="Jan-Willem Spuij">
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

namespace RedCow.Immutable
{
    using System;
    using System.Reflection;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides cloning using reflection.
    /// </summary>
    public class ReflectionCloneProvider : ICloneProvider
    {
        /// <summary>
        /// Clones the public properties of an object to another object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destinationActivator">The function that creates the destination instance..</param>
        /// <param name="assignmentFunction">The assignment function to use to assign the source value properties to the destination.</param>
        /// <returns>The cloned type.</returns>
        public object Clone(object source, Func<object> destinationActivator, Func<object?, object?> assignmentFunction)
        {
            var sourceType = source.GetType();
            object destination = destinationActivator();
            var destinationType = destination.GetType();

            foreach (var sourceProperty in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var destinationProperty = destinationType.GetProperty(sourceProperty.Name);

                if (destinationProperty == null || destinationProperty.SetMethod == null)
                {
                    continue;
                }

                object? value = sourceProperty.GetValue(source);
                value = assignmentFunction(value);
                destinationProperty.SetValue(destination, value);
            }

            return destination;
        }
    }
}
