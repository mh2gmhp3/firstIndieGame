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

        public static void MoveToNext<T>(this IList<T> list, int index)
        {
            if (list == null)
                return;
            if (index >= list.Count || index < 0)
                return;

            if (index + 1 < list.Count)
            {
                var temp = list[index + 1];
                list[index + 1] = list[index];
                list[index] = temp;
            }
            else
            {
                var temp = list[index];
                //刪除原本的
                list.RemoveAt(index);
                //插入第一個
                list.Insert(0, temp);
            }
        }

        public static void MoveToPrevious<T>(this IList<T> list, int index)
        {
            if (list == null)
                return;
            if (index >= list.Count || index < 0)
                return;

            if (index - 1 > 0)
            {
                var temp = list[index - 1];
                list[index - 1] = list[index];
                list[index] = temp;
            }
            else
            {
                var temp = list[index];
                //刪除原本的
                list.RemoveAt(index);
                //加入最後
                list.Add(temp);
            }
        }
    }
}
