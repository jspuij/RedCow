// <copyright file="DraftExtensions.cs" company="Jan-Willem Spuij">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using RedCow.Immutable.Collections;

    /// <summary>
    /// Extension methods for drafts.
    /// </summary>
    public static class DraftExtensions
    {
        /// <summary>
        /// Creates a new Draft, based on the type of the state.
        /// </summary>
        /// <typeparam name="T">The type of the state.</typeparam>
        /// <param name="state">The immutable.</param>
        /// <param name="draft">A new draft, based on the immutable.</param>
        /// <param name="producerOptions">The producer options to use. If you leave them null, the default options will be used.</param>
        /// <returns>A scope that is used to either reconcile or dispose of the draft.</returns>
        public static IDraftScope CreateDraft<T>(this T state, out T draft, IProducerOptions? producerOptions = null)
            where T : class
        {
            return InternalCreateDraft<T>(state, out draft, producerOptions);
        }

        /// <summary>
        /// Tests whether an object is a draft.
        /// </summary>
        /// <typeparam name="T">The type to test.</typeparam>
        /// <param name="state">The state to test.</param>
        /// <returns>A value indicating whether the object is a draft.</returns>
        public static bool IsDraft<T>(this T state)
            where T : class
        {
            return InternalIsDraft(state);
        }

        /// <summary>
        /// Tests whether an object is a draftable.
        /// </summary>
        /// <typeparam name="T">The type to test.</typeparam>
        /// <param name="state">The state to test.</param>
        /// <returns>A value indicating whether the object is draftable.</returns>
        public static bool IsDraftable<T>(this T state)
            where T : class
        {
            return InternalIsDraftable(state);
        }

        /// <summary>
        /// Creates a new Draft, based on the type of the state.
        /// </summary>
        /// <typeparam name="T">The type of the state.</typeparam>
        /// <param name="state">The immutable.</param>
        /// <param name="draft">A new draft, based on the immutable.</param>
        /// <param name="producerOptions">The producer options to use. If you leave them null, the default options will be used.</param>
        /// <returns>A scope that is used to either reconcile or dispose of the draft.</returns>
        internal static IDraftScope InternalCreateDraft<T>(object state, out T draft, IProducerOptions? producerOptions = null)
            where T : class
        {
            var scope = new DraftScope(producerOptions ?? ProducerOptions.Default);

            try
            {
                var draftState = InternalGetDraftState(state);
                if (draftState != null)
                {
                    scope.Parent = draftState.Scope;
                }

                draft = (T)scope.CreateProxy(state);
                return scope;
            }
            catch
            {
                scope.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Gets the Draft state or returns null otherwise.
        /// </summary>
        /// <param name="state">The state to test.</param>
        /// <returns>The draft state for the object.</returns>
        internal static DraftState? InternalGetDraftState(object state)
        {
            return (state as IDraft)?.DraftState;
        }

        /// <summary>
        /// Tests whether an object is a draft.
        /// </summary>
        /// <param name="state">The state to test.</param>
        /// <returns>A value indicating whether the object is a draft.</returns>
        internal static bool InternalIsDraft(object state)
        {
            return state is IDraft idraft && idraft.DraftState != null;
        }

        /// <summary>
        /// Tests whether an object is a draftable.
        /// </summary>
        /// <param name="state">The state to test.</param>
        /// <returns>A value indicating whether the object is draftable.</returns>
        /// <exception cref="ArgumentNullException">when the state is null.</exception>
        internal static bool InternalIsDraftable(object state)
        {
            if (state is null)
            {
                return false;
            }

            return GetProxyType(state) != null;
        }

        /// <summary>
        /// Gets the proxy Type for this object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The draft type if found, otherwise null.</returns>
        internal static Type? GetProxyType(object source)
        {
            Type? type = source.GetType();

            if (source is IEnumerable)
            {
                if (type.IsGenericType)
                {
                    var genericTypeArguments = type.GetGenericArguments();

                    if (genericTypeArguments.Length == 2)
                    {
                        var dictionaryType = typeof(IDictionary<,>).MakeGenericType(genericTypeArguments);
                        if (dictionaryType.IsAssignableFrom(type))
                        {
                            return typeof(ProxyDictionary<,>).MakeGenericType(genericTypeArguments);
                        }

                        throw new NotImplementedException();
                    }
                    else if (genericTypeArguments.Length == 1)
                    {
                        var setType = typeof(ISet<>).MakeGenericType(genericTypeArguments);
                        var collectionType = typeof(ICollection<>).MakeGenericType(genericTypeArguments);
                        if (setType.IsAssignableFrom(type))
                        {
                            throw new NotImplementedException();
                        }
                        else if (collectionType.IsAssignableFrom(type))
                        {
                            return typeof(ProxyList<>).MakeGenericType(genericTypeArguments);
                        }

                        throw new NotImplementedException();
                    }
                }
                else
                {
                    if (source is IList || source is ICollection)
                    {
                        return typeof(ProxyList<object>);
                    }
                }
            }

            Type? draftType = null;

            while (type != null)
            {
                if (type.GetCustomAttributes().SingleOrDefault(x => x is ProxyTypeAttribute) is ProxyTypeAttribute draftTypeAttribute)
                {
                    draftType = draftTypeAttribute.ProxyType;
                    break;
                }

                type = type.BaseType;
            }

            return draftType;
        }
    }
}
