using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extension
{
    public static class ListExtension
    {
        public static bool TryGet<T>(this IList<T> list, int index, out T result)
        {
            result = default(T);
            if (list == null)
                return false;

            if (index < 0 || index >= list.Count)
                return false;

            result = list[index];
            return true;
        }

        public static bool TryGetFirst<T>(this IList<T> list, out T result)
        {
            return TryGet(list, 0, out result);
        }
    }
}
