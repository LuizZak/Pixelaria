using System.Collections.Generic;

namespace Pixelaria.Utils
{
    /// <summary>
    /// Collection of List utility extension methods
    /// </summary>
    public static class ListUtils
    {
        /// <summary>
        /// Returns whether a list contains the reference of one object in it
        /// </summary>
        /// <typeparam name="T">The type of object contained in the list</typeparam>
        /// <param name="list">The list of items to search in</param>
        /// <param name="obj">The object to find in the list</param>
        /// <returns>True if the item is in the list, false otherwise</returns>
        public static bool ContainsReference<T>(this List<T> list, T obj)
        {
            return list.IndexOfReference(obj) != -1;
        }

        /// <summary>
        /// Removes an element from an array by reference and returns whether the item was in the list
        /// </summary>
        /// <typeparam name="T">The type of object contained in the list</typeparam>
        /// <param name="list">The list of items to operate on</param>
        /// <param name="obj">The object to remove from the list</param>
        /// <returns>True if the item was in the list and was removed, false otherwise</returns>
        public static bool RemoveReference<T>(this List<T> list, T obj)
        {
            int index = list.IndexOfReference(obj);

            if (index == -1)
                return false;

            list.RemoveAt(index);

            return true;
        }

        /// <summary>
        /// Returns the index of the first item that matches the reference of another given object
        /// </summary>
        /// <typeparam name="T">The type of object contained in the list</typeparam>
        /// <param name="list">The list of items to search in</param>
        /// <param name="obj">The object to find in the list</param>
        /// <returns>Returns a zero-based index of the first item that matches the reference of another object, or -1, if none is found</returns>
        public static int IndexOfReference<T>(this List<T> list, T obj)
        {
            return list.FindIndex(item => ReferenceEquals(item, obj));
        }
    }
}