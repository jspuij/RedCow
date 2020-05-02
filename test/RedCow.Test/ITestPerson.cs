// <copyright file="ITestPerson.cs" company="Jan-Willem Spuij">
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

    /// <summary>
    /// Interface for an Immutable Test Person.
    /// </summary>
    public partial interface ITestPerson : Immutable<TestPerson>
    {
        /// <summary>
        /// Gets the first name.
        /// </summary>
        string FirstName { get; }

        /// <summary>
        /// Gets the last name.
        /// </summary>
        public string LastName { get; }

        /// <summary>
        /// Gets a value indicating whether isAdult is true.
        /// </summary>
        public bool IsAdult { get;  }

        /// <summary>
        /// Produces the next <see cref="Immutable{T}"/> based on the
        /// specified producer function.
        /// </summary>
        /// <param name="producer">The producer action.</param>
        /// <param name="cloneProvider">The clone provider to use.</param>
        /// <returns>The next immutable state.</returns>
        public ITestPerson Produce(Action<TestPerson> producer, ICloneProvider cloneProvider = null) =>
            Produce((TestPerson)this, producer, cloneProvider);

        /// <summary>
        /// Produces the next <see cref="Immutable{T}"/> based on the
        /// specified producer function.
        /// </summary>
        /// <param name="initialState">The initial State.</param>
        /// <param name="producer">The producer action.</param>
        /// <param name="cloneProvider">The clone provider to use.</param>
        /// <returns>The next immutable state.</returns>
        public static ITestPerson Produce(TestPerson initialState, Action<TestPerson> producer, ICloneProvider cloneProvider = null) =>
            Producer(producer, cloneProvider)(initialState);

        /// <summary>
        /// Produces the next <see cref="Immutable{T}"/> based on the
        /// specified producer function.
        /// </summary>
        /// <param name="producer">The producer function.</param>
        /// <param name="cloneProvider">The clone provider to use.</param>
        /// <returns>The next immutable state.</returns>
        public ITestPerson Produce(Func<TestPerson> producer, ICloneProvider cloneProvider = null) =>
            Produce((TestPerson)this, producer, cloneProvider);

        /// <summary>
        /// Produces the next <see cref="Immutable{T}"/> based on the
        /// specified producer function.
        /// </summary>
        /// <param name="initialState">The initial State.</param>
        /// <param name="producer">The producer function.</param>
        /// <param name="cloneProvider">The clone provider to use.</param>
        /// <returns>The next immutable state.</returns>
        public static ITestPerson Produce(TestPerson initialState, Func<TestPerson> producer, ICloneProvider cloneProvider = null) =>
            Producer(producer, cloneProvider)(initialState);

        /// <summary>
        /// Creates a Producer delegate that can be used to curry on an Immutable State.
        /// </summary>
        /// <param name="producer">The producer action that operates on objects of type T.</param>
        /// <param name="cloneProvider">The clone provider to use.</param>
        /// <returns>A producer delegate.</returns>
        public static Func<TestPerson, ITestPerson> Producer(Action<TestPerson> producer, ICloneProvider cloneProvider = null) =>
            (imm1) => Producer<object>((arg1, _) => producer(arg1), cloneProvider)(imm1, null);

        /// <summary>
        /// Creates a Producer delegate that can be used to curry on an Immutable State.
        /// </summary>
        /// <param name="producer">The producer function that operates on objects of type T.</param>
        /// <param name="cloneProvider">The clone provider to use.</param>
        /// <returns>A producer delegate.</returns>
        public static Func<TestPerson, ITestPerson> Producer(Func<TestPerson> producer, ICloneProvider cloneProvider = null) =>
            (imm1) => Producer<object>(_ => producer(), cloneProvider)(imm1, null);

        /// <summary>
        /// Creates a Producer delegate that can be used to curry on an Immutable State.
        /// </summary>
        /// <param name="producer">The producer action that operates on objects of type T with a single argument.</param>
        /// <param name="cloneProvider">The clone provider to use.</param>
        /// <typeparam name="TArg">The type of the argument.</typeparam>
        /// <returns>A producer delegate.</returns>
        public static Func<TestPerson, TArg, ITestPerson> Producer<TArg>(Action<TestPerson, TArg> producer, ICloneProvider cloneProvider = null) =>
            (imm1, arg1) =>
            {
                using var scope = imm1.CreateDraft(out var draft, cloneProvider);
                producer((TestPerson)draft, arg1);
                return (ITestPerson)scope.FinishDraft((TestPerson)draft);
            };

        /// <summary>
        /// Creates a Producer delegate that can be used to curry on an Immutable State.
        /// </summary>
        /// <param name="producer">The producer function that operates on objects of type T with a single argument.</param>
        /// <param name="cloneProvider">The clone provider to use.</param>
        /// <typeparam name="TArg">The type of the argument.</typeparam>
        /// <returns>A producer delegate.</returns>
        public static Func<TestPerson, TArg, ITestPerson> Producer<TArg>(Func<TArg, TestPerson> producer, ICloneProvider cloneProvider = null) =>
            (imm1, arg1) =>
            {
                using var scope = imm1.CreateDraft(out var _, cloneProvider);
                TestPerson draft = producer(arg1);
                return (ITestPerson)scope.FinishDraft(draft);
            };
    }
}
