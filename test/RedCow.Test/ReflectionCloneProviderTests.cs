// <copyright file="ReflectionCloneProviderTests.cs" company="Jan-Willem Spuij">
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

namespace RedCow.Test
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="ReflectionCloneProvider"/>.
    /// </summary>
    public class ReflectionCloneProviderTests
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1600 // Elements should be documented
        public string PublicField;

        private string privateField;

        public string PublicProperty { get; set; }

        public string ReadOnlyProperty => this.privateField;

        private string PrivateProperty { get; set; }
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Tests whether the Clone provider clones all properties.
        /// </summary>
        [Fact]
        public void ClonesAllPublicProperties()
        {
            var cloneProvider = new ReflectionCloneProvider();
            var destination = new ReflectionCloneProviderTests();

            this.PublicProperty = "John Doe";

            cloneProvider.Clone(this, destination);

            Assert.Equal(this.PublicProperty, destination.PublicProperty);
        }

        /// <summary>
        /// Tests that the clone provider does not clone fields, private and readonly properties.
        /// </summary>
        [Fact]
        public void DoesNotClonePrivatePropertiesAndFields()
        {
            var cloneProvider = new ReflectionCloneProvider();
            var destination = new ReflectionCloneProviderTests();

            this.privateField = "John Doe";
            this.PublicField = "John Doe";
            this.PrivateProperty = "John Doe";

            cloneProvider.Clone(this, destination);

            Assert.NotEqual(this.privateField, destination.privateField);
            Assert.NotEqual(this.PublicField, destination.PublicField);
            Assert.NotEqual(this.PrivateProperty, destination.PrivateProperty);
            Assert.NotEqual(this.ReadOnlyProperty, destination.ReadOnlyProperty);
        }
    }
}
