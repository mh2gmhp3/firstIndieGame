using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extension
{
    public static class BoundsExtension
    {
        public static bool IntersectRayAABB(
            this Bounds bounds,
            Ray ray,
            float distance,
            out float tDistance)
        {
            tDistance = 0;

            var min = bounds.min;
            var max = bounds.max;

            float tEnter = float.MinValue;
            //最遠
            float tExit = distance;

            Vector3 origin = ray.origin;

            // x y z
            for (int i = 0; i < 3; i++)
            {
                float rayDir = ray.direction[i];
                float rayOrigin = origin[i];
                float minBound = min[i];
                float maxBound = max[i];

                // dir 0
                if (rayDir.ApproximateEqual(0))
                {
                    if (rayOrigin < minBound || rayOrigin > maxBound)
                    {
                        return false; // 射線平行於軸，且在範圍外
                    }
                    // 射線平行且在範圍內，tEnter, tExit 不變，繼續下一個軸
                    continue;
                }

                float t1 = (minBound - rayOrigin) / rayDir;
                float t2 = (maxBound - rayOrigin) / rayDir;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                // 更新全域 tEnter (取最大進入值) 和 tExit (取最小離開值)
                tEnter = Mathf.Max(tEnter, t1);
                tExit = Mathf.Min(tExit, t2);

                // 如果進入點超過離開點，或者離開點在起點後面，則無相交
                if (tEnter > tExit || tExit < 0)
                    return false;
            }

            // 確保最終進入點 tEnter 不會小於 0 (處理起點在 AABB 內的情況)
            tEnter = Mathf.Max(0, tEnter);

            // 額外檢查：如果進入點 tEnter 已經超過了最大允許距離 tExit (maxDistance)，則返回 false
            if (tEnter > tExit)
                return false;

            tDistance = tEnter;
            return true;
        }
    }
}
