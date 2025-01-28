using System;
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

        public static void RemoveIf<T>(this IList<T> list, Func<T, bool> conditional)
        {
            if (list == null || conditional == null)
                return;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!conditional.Invoke(list[i]))
                    continue;

                list.RemoveAt(i);
            }
        }
    }
}
