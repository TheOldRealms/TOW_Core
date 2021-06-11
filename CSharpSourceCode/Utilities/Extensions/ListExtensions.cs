using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOW_Core.Utilities.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Add an object to the list if the object is not null.
        /// 
        /// Cleaner than having the conditional in every place we want to follow this pattern.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list being added to</param>
        /// <param name="item">The object to be added to the list</param>
        public static void AddIfNotNull<T>(this List<T> list, T item)
        {
            if (item != null)
            {
                list.Add(item);
            }
        }

        public static void RemoveIfExists<T>(this IEnumerable<T> list, T item) where T : class
        {
            list = list.Where(x => x != item);
        }

        public static void RemoveAllOfType<T>(this IEnumerable<T> list, Type type) where T : class
        {
            list = list.Where(x => x.GetType() != type);

        }
    }
}
