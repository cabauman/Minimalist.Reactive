﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Minimalist.Reactive;
using Minimalist.Reactive.Linq;
using System.Text;
using Xunit;

namespace Minimalist.Reactive.Testing
{
    /// <summary>
    /// Helper class to write asserts in unit tests for applications and libraries built using Reactive Extensions.
    /// </summary>
    public static class ReactiveAssert
    {
        private static string Message<T>(IEnumerable<T> actual, IEnumerable<T> expected)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.Append("Expected: [");
            sb.Append(string.Join(", ", expected.Select(x => x.ToString()).ToArray()));
            sb.Append(']');
            sb.AppendLine();
            sb.Append("Actual..: [");
            sb.Append(string.Join(", ", actual.Select(x => x.ToString()).ToArray()));
            sb.Append(']');
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Asserts that both enumerable sequences have equal length and equal elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="expected">Expected sequence.</param>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AreElementsEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual) => AreElementsEqual(expected, actual, EqualityComparer<T>.Default);

        /// <summary>
        /// Asserts that both enumerable sequences have equal length and equal elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="expected">Expected sequence.</param>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <param name="comparer">Comparer used to check for equality.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AreElementsEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            if (!expected.SequenceEqual(actual, comparer ?? EqualityComparer<T>.Default))
            {
                Assert.True(false, Message(actual, expected));
            }
        }

        /// <summary>
        /// Asserts that both enumerable sequences have equal length and equal elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="expected">Expected sequence.</param>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <param name="message">Error message for assert failure.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AreElementsEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message) => AreElementsEqual(expected, actual, EqualityComparer<T>.Default, message);

        /// <summary>
        /// Asserts that both enumerable sequences have equal length and equal elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="expected">Expected sequence.</param>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <param name="comparer">Comparer used to check for equality.</param>
        /// <param name="message">Error message for assert failure.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AreElementsEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer, string message)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            if (!expected.SequenceEqual(actual, comparer ?? EqualityComparer<T>.Default))
            {
                Assert.True(false, message);
            }
        }

        /// <summary>
        /// Asserts that both observable sequences have equal length and equal notifications.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="expected">Expected sequence.</param>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AreElementsEqual<T>(IObservable<T> expected, IObservable<T> actual)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            AreElementsEqual(expected.Materialize().ToEnumerable(), actual.Materialize().ToEnumerable());
        }

        /// <summary>
        /// Asserts that both observable sequences have equal length and equal notifications.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="expected">Expected sequence.</param>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <param name="comparer">Comparer used to check for equality.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AreElementsEqual<T>(IObservable<T> expected, IObservable<T> actual, IEqualityComparer<T> comparer)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            AreElementsEqual(expected.Materialize().ToEnumerable(), actual.Materialize().ToEnumerable(), new NotificationComparer<T>(comparer));
        }

        /// <summary>
        /// Asserts that both observable sequences have equal length and equal elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="expected">Expected sequence.</param>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <param name="message">Error message for assert failure.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AreElementsEqual<T>(IObservable<T> expected, IObservable<T> actual, string message)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            AreElementsEqual(expected.Materialize().ToEnumerable(), actual.Materialize().ToEnumerable(), message);
        }

        /// <summary>
        /// Asserts that both observable sequences have equal length and equal elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="expected">Expected sequence.</param>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <param name="comparer">Comparer used to check for equality.</param>
        /// <param name="message">Error message for assert failure.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AreElementsEqual<T>(IObservable<T> expected, IObservable<T> actual, IEqualityComparer<T> comparer, string message)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            AreElementsEqual(expected.Materialize().ToEnumerable(), actual.Materialize().ToEnumerable(), new NotificationComparer<T>(comparer), message);
        }

        /// <summary>
        /// Asserts that the given action throws an exception of the type specified in the generic parameter, or a subtype thereof.
        /// </summary>
        /// <typeparam name="TException">Type of the exception to check for.</typeparam>
        /// <param name="action">Action to run.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public static void Throws<TException>(Action action) where TException : Exception
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var failed = false;
            try
            {
                action();
                failed = true;
            }
            catch (TException)
            {
            }
            catch (Exception ex)
            {
                Assert.True(false, string.Format(CultureInfo.CurrentCulture, "Expected {0} threw {1}.\r\n\r\nStack trace:\r\n{2}", typeof(TException).Name, ex.GetType().Name, ex.StackTrace));
            }

            if (failed)
            {
                Assert.True(false, string.Format(CultureInfo.CurrentCulture, "Expected {0}.", typeof(TException).Name));
            }
        }

        /// <summary>
        /// Asserts that the given action throws an exception of the type specified in the generic parameter, or a subtype thereof.
        /// </summary>
        /// <typeparam name="TException">Type of the exception to check for.</typeparam>
        /// <param name="action">Action to run.</param>
        /// <param name="message">Error message for assert failure.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public static void Throws<TException>(Action action, string message) where TException : Exception
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var failed = false;
            try
            {
                action();
                failed = true;
            }
            catch (TException)
            {
            }
            catch
            {
                Assert.True(false, message);
            }

            if (failed)
            {
                Assert.True(false, message);
            }
        }

        /// <summary>
        /// Asserts that the given action throws the specified exception.
        /// </summary>
        /// <typeparam name="TException">Type of the exception to check for.</typeparam>
        /// <param name="exception">Exception to assert being thrown.</param>
        /// <param name="action">Action to run.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public static void Throws<TException>(TException exception, Action action) where TException : Exception
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var failed = false;
            try
            {
                action();
                failed = true;
            }
            catch (TException ex)
            {
                Assert.Same(exception, ex);
            }
            catch (Exception ex)
            {
                Assert.True(false, string.Format(CultureInfo.CurrentCulture, "Expected {0} threw {1}.\r\n\r\nStack trace:\r\n{2}", typeof(TException).Name, ex.GetType().Name, ex.StackTrace));
            }

            if (failed)
            {
                Assert.True(false, string.Format(CultureInfo.CurrentCulture, "Expected {0}.", typeof(TException).Name));
            }
        }

        /// <summary>
        /// Asserts that the given action throws the specified exception.
        /// </summary>
        /// <typeparam name="TException">Type of the exception to check for.</typeparam>
        /// <param name="exception">Exception to assert being thrown.</param>
        /// <param name="action">Action to run.</param>
        /// <param name="message">Error message for assert failure.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public static void Throws<TException>(TException exception, Action action, string message) where TException : Exception
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var failed = false;
            try
            {
                action();
                failed = true;
            }
            catch (TException ex)
            {
                Assert.Same(exception, ex);
            }
            catch
            {
                Assert.True(false, message);
            }

            if (failed)
            {
                Assert.True(false, message);
            }
        }

        /// <summary>
        /// Asserts that both enumerable sequences have equal length and equal elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <param name="expected">Expected sequence.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AssertEqual<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
        {
            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            AreElementsEqual(expected, actual);
        }

        /// <summary>
        /// Asserts that both enumerable sequences have equal length and equal elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <param name="expected">Expected sequence.</param>
        /// <param name="comparer">Comparer used to check for equality.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AssertEqual<T>(this IEnumerable<T> actual, IEnumerable<T> expected, IEqualityComparer<T> comparer)
        {
            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            AreElementsEqual(expected, actual, comparer);
        }

        /// <summary>
        /// Asserts the enumerable sequence has the expected elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="actual">Actual sequence to compare against the expected elements.</param>
        /// <param name="expected">Expected elements.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AssertEqual<T>(this IEnumerable<T> actual, params T[] expected)
        {
            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            AreElementsEqual(expected, actual);
        }

        /// <summary>
        /// Asserts that both observable sequences have equal length and equal notifications.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <param name="expected">Expected sequence.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AssertEqual<T>(this IObservable<T> actual, IObservable<T> expected)
        {
            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            AreElementsEqual(expected, actual);
        }

        /// <summary>
        /// Asserts that both observable sequences have equal length and equal notifications.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="actual">Actual sequence to compare against the expected one.</param>
        /// <param name="expected">Expected sequence.</param>
        /// <param name="comparer">Comparer used to check for equality.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AssertEqual<T>(this IObservable<T> actual, IObservable<T> expected, IEqualityComparer<T> comparer)
        {
            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            AreElementsEqual(expected, actual, comparer);
        }

        private sealed class NotificationComparer<T> : IEqualityComparer<Notification<T>>
        {
            private readonly IEqualityComparer<T> _comparer;

            public NotificationComparer(IEqualityComparer<T> comparer)
            {
                _comparer = comparer;
            }

            public bool Equals(Notification<T> x, Notification<T> y)
            {
                if (x != null && y != null)
                {
                    if (x.HasValue && y.HasValue)
                    {
                        return _comparer.Equals(x.Value, y.Value);
                    }
                }

                return EqualityComparer<Notification<T>>.Default.Equals(x, y);
            }

            public int GetHashCode(Notification<T> obj)
            {
                if (obj != null && obj.HasValue)
                {
                    return _comparer.GetHashCode(obj.Value);
                }

                return EqualityComparer<Notification<T>>.Default.GetHashCode(obj);
            }
        }
    }
}
