// <copyright file="Store.cs" company="Jan-Willem Spuij">
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

namespace RedCow
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Implements a store.
    /// </summary>
    /// <typeparam name="T">The type of the State in the store.</typeparam>
    public class Store<T> : IStore<T>
    {
        /// <summary>
        /// The reducer that is called on dispatch.
        /// </summary>
        private readonly Func<T, object, T> reducer;

        /// <summary>
        /// A boolean indicating that we are dispatching.
        /// </summary>
        private bool dispatching = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Store{T}"/> class.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        /// <param name="reducer">The root reducer.</param>
        private Store(T initialState, Func<T, object, T> reducer)
        {
            this.State = initialState;
            this.reducer = reducer;
        }

        /// <summary>
        /// Gets the State.
        /// </summary>
        public T State { get; private set; }

        /// <summary>
        /// Dispatches an action.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        public void Dispatch(object action)
        {
            try
            {
                this.dispatching = true;
                this.State = this.reducer(this.State, action);
            }
            finally
            {
                this.dispatching = false;
            }
        }
    }
}
