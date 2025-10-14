using System;
using System.Collections.Generic;

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

        /// <summary>
        /// 確保List數量 要移除超過的數量需啟用removeExceed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <param name="defaultValue"></param>
        public static void EnsureCount<T>(this IList<T> list, int count, Func<T> defaultValue, bool removeExceed = false)
        {
            if (list == null)
                return;

            if (defaultValue == null)
                return;

            if (count < 0)
                return;

            var diffCount = count - list.Count;
            if (diffCount > 0)
            {
                for (int i = 0; i < diffCount; i++)
                {
                    list.Add(defaultValue.Invoke());
                }
            }
            else if (diffCount < 0 && removeExceed)
            {
                diffCount = Math.Abs(diffCount);
                for (int i = 0; i < diffCount; i++)
                {
                    list.RemoveAt(list.Count - 1);
                }
            }
        }
    }
}
