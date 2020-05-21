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
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Implements a store.
    /// </summary>
    /// <typeparam name="T">The type of the State in the store.</typeparam>
    public class Store<T> : IStore<T>, IObservable<T>, IDispatch
    {
        /// <summary>
        /// An action that is dispatched at the start to make sure that the
        /// store is initialized by calling all the reducers.
        /// </summary>
        private const string Init = "RedCow.X.Store.Init";

        /// <summary>
        /// The reducer that is called on dispatch.
        /// </summary>
        private readonly Func<T, object, T> reducer;

        /// <summary>
        /// Subscriptions to this observable.
        /// </summary>
        private readonly HashSet<IObserver<T>> subscriptions = new HashSet<IObserver<T>>();

        /// <summary>
        /// A boolean indicating that the subscriptions changed.
        /// </summary>
        private bool subscriptionsChanged = false;

        /// <summary>
        /// A copy of the subscriptions that is used to iterate over.
        /// </summary>
        private IObserver<T>[]? subscriptionsToNotify;

        /// <summary>
        /// A boolean indicating that we are dispatching.
        /// </summary>
        private bool dispatching = false;

        /// <summary>
        /// A reference to the store state.
        /// </summary>
        private T state;

        /// <summary>
        /// Initializes a new instance of the <see cref="Store{T}"/> class.
        /// </summary>
        /// <param name="reducer">The root reducer.</param>
        public Store(Func<T, object, T> reducer)
            : this(default!, reducer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Store{T}"/> class.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        /// <param name="reducer">The root reducer.</param>
        public Store(T initialState, Func<T, object, T> reducer)
        {
            this.state = initialState;
            this.reducer = reducer ?? throw new ArgumentNullException(nameof(reducer));

            this.Dispatch(Init);
        }

        /// <summary>
        /// Gets the State.
        /// </summary>
        public T State
        {
            get
            {
                if (this.dispatching)
                {
                    throw new DispatchException("Cannot get the State while dispatching.");
                }

                return this.state;
            }
        }

        /// <summary>
        /// Dispatches an action.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        public void Dispatch(object action)
        {
            if (this.dispatching)
            {
                throw new DispatchException("Dispatching actions from reducers is not allowed.");
            }

            try
            {
                this.dispatching = true;
                this.state = this.reducer(this.state, action);
            }
            finally
            {
                this.dispatching = false;
            }

            // copy the list so that the enumerator is always valid, even
            // if a subscription is removed or added during iteration.
            if (this.subscriptionsToNotify == null || this.subscriptionsChanged)
            {
                this.subscriptionsToNotify = this.subscriptions.ToArray();
                this.subscriptionsChanged = false;
            }

            foreach (var observer in this.subscriptionsToNotify)
            {
                observer.OnNext(this.state);
            }
        }

        /// <summary>
        /// Notifies the store that an observer is ready to receive notifications.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <returns>A disposable that can be used to cancel the subscription.</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer is null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            if (this.dispatching)
            {
                throw new DispatchException("Adding subscriptions during Dispatching is not allowed.");
            }

            this.subscriptionsChanged = true;
            this.subscriptions.Add(observer);
            return new DelegateDisposable(() =>
            {
                if (this.dispatching)
                {
                    throw new DispatchException("Removing subscriptions during Dispatching is not allowed.");
                }

                this.subscriptionsChanged = true;
                this.subscriptions.Remove(observer);
            });
        }
    }
}
