// <copyright file="DelegateObserver{T}.cs" company="Jan-Willem Spuij">
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

namespace RedCow.X.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// A class that fires delegates when the methods
    /// of IObserver<typeparamref name="T"/>> are fired.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    [ExcludeFromCodeCoverage]
    public class DelegateObserver<T> : IObserver<T>
    {
        private readonly Action completedAction;
        private readonly Action<Exception> errorAction;
        private readonly Action<T> valueAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateObserver{T}"/> class.
        /// </summary>
        /// <param name="valueAction">The value function.</param>
        public DelegateObserver(Action<T> valueAction)
            : this(() => { }, e => { }, valueAction)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateObserver{T}"/> class.
        /// </summary>
        /// <param name="completedAction">The action when the delegate is completed.</param>
        /// <param name="errorAction">The error action.</param>
        /// <param name="valueAction">The value function.</param>
        public DelegateObserver(Action completedAction, Action<Exception> errorAction, Action<T> valueAction)
        {
            this.completedAction = completedAction ?? throw new ArgumentNullException(nameof(completedAction));
            this.errorAction = errorAction ?? throw new ArgumentNullException(nameof(errorAction));
            this.valueAction = valueAction ?? throw new ArgumentNullException(nameof(valueAction));
        }

        /// <summary>
        /// Notifies the observer that the observable finished.
        /// </summary>
        public void OnCompleted()
        {
            this.completedAction();
        }

        /// <summary>
        /// Notifies the observer that an error occured.
        /// </summary>
        /// <param name="error">The error.</param>
        public void OnError(Exception error)
        {
            this.errorAction(error);
        }

        /// <summary>
        /// Notifies the observer of the next value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void OnNext(T value)
        {
            this.valueAction(value);
        }
    }
}
