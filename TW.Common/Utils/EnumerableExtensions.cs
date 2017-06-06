namespace TW.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     The enumerable extensions.
    /// </summary>
    public static class EnumerableExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Searches for an element that matches the conditions defined by the specified predicate, and returns the first
        ///     occurrence within the entire IEnumerable.
        /// </summary>
        /// <typeparam name="TSource">
        ///     The source type.
        /// </typeparam>
        /// <param name="source">
        ///     The source.
        /// </param>
        /// <param name="match">
        ///     The match.
        /// </param>
        /// <returns></returns>
        public static TSource Find<TSource>(this IEnumerable<TSource> source, Predicate<TSource> match)
        {
            return (source as List<TSource> ?? source.ToList()).Find(match);
        }

        /// <summary>
        ///     Determines if a list contains any of the values.
        /// </summary>
        /// <param name="source">Container of objects</param>
        /// <param name="list">Any object that should be in the container</param>
        /// <typeparam name="T">Type of object to look for</typeparam>
        /// <returns>If the container contains any values.</returns>
        public static bool In<T>(this T source, params T[] list)
        {
            return list.Contains(source);
        }

        /// <summary>
        ///     Does an action over a list of types.
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="list">List of values</param>
        /// <param name="action">Function to call foreach value</param>
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
        }

        /// <summary>
        ///     Searches for the max or default element.
        /// </summary>
        /// <typeparam name="T">
        ///     The type.
        /// </typeparam>
        /// <typeparam name="TR">
        ///     The comparing type.
        /// </typeparam>
        /// <param name="container">
        ///     The container.
        /// </param>
        /// <param name="valuingFoo">
        ///     The comparing function.
        /// </param>
        /// <returns></returns>
        public static T MaxOrDefault<T, TR>(this IEnumerable<T> container, Func<T, TR> valuingFoo)
            where TR : IComparable
        {
            var enumerator = container.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return default(T);
            }

            var maxElem = enumerator.Current;
            var maxVal = valuingFoo(maxElem);

            while (enumerator.MoveNext())
            {
                var currVal = valuingFoo(enumerator.Current);

                if (currVal.CompareTo(maxVal) > 0)
                {
                    maxVal = currVal;
                    maxElem = enumerator.Current;
                }
            }

            return maxElem;
        }

        /// <summary>
        ///     Searches for the min or default element.
        /// </summary>
        /// <typeparam name="T">
        ///     The type.
        /// </typeparam>
        /// <typeparam name="TR">
        ///     The comparing type.
        /// </typeparam>
        /// <param name="container">
        ///     The container.
        /// </param>
        /// <param name="valuingFoo">
        ///     The comparing function.
        /// </param>
        /// <returns></returns>
        public static T MinOrDefault<T, TR>(this IEnumerable<T> container, Func<T, TR> valuingFoo)
            where TR : IComparable
        {
            var enumerator = container.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return default(T);
            }

            var minElem = enumerator.Current;
            var minVal = valuingFoo(minElem);

            while (enumerator.MoveNext())
            {
                var currVal = valuingFoo(enumerator.Current);

                if (currVal.CompareTo(minVal) < 0)
                {
                    minVal = currVal;
                    minElem = enumerator.Current;
                }
            }

            return minElem;
        }

        #endregion
    }
}