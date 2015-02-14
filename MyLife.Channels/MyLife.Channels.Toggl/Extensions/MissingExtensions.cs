using System;
using System.Collections.Generic;
using System.Net;

namespace Toggl
{
    public static class MissingExtensions
    {
        /// <summary>
        /// Performs the specified action on each element of the System.Collections.Generic.List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="action">The System.Action delegate to perform on each element of the System.Collections.Generic.List</param>
        /// <returns></returns>
        public static void ForEach<T>(this IList<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
        }


        /// <summary>
        /// Inserts the specified header into the collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="header">The header to add, with the name and value separated by a colon.</param>
        public static void Add(this WebHeaderCollection collection, string header)
        {
            if (header == null)
                throw new ArgumentNullException("header");

            var parts = header.Split(':');
            var name = parts[0];
            var value = parts[1];
            collection[name] = value;
        }

    }
}
