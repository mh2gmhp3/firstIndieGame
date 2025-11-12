using Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public static class VoxelUtility
    {
        public struct RaycastResult
        {
            public Vector3Int Coordinates;
            public float Distance;
            /// <summary>
            /// 射線投射至Voxel的方向
            /// </summary>
            public Vector3Int HitRayNormal;
        }

        /// <summary>
        /// 使用(0, 0, 0)原點進行DDA 獲取射線經過的方格
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="size"></param>
        /// <param name="currentDis"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static IEnumerable<RaycastResult> RaycastDDA(
            Ray ray,
            Vector3 size,
            float currentDis,
            float distance)
        {
            float tCurrent = currentDis;

            var direction = ray.direction;

            var currentPosition = ray.GetPoint(currentDis);

            var coord = new Vector3Int(
                Mathf.FloorToInt(currentPosition.x / size.x),
                Mathf.FloorToInt(currentPosition.y / size.y),
                Mathf.FloorToInt(currentPosition.z / size.z));

            var step = new Vector3Int(
                Math.Sign(direction.x),
                Math.Sign(direction.y),
                Math.Sign(direction.z));

            Vector3 tDelta = new Vector3();
            for (int i = 0; i < 3; i++)
            {
                if (direction[i].ApproximateEqual(0))
                {
                    tDelta[i] = Single.PositiveInfinity;
                }
                else
                {
                    tDelta[i] = Mathf.Abs(size[i] / direction[i]);
                }
            }

            Vector3 tMax = new Vector3();
            for (int i = 0; i < 3; i++)
            {
                float currentStep = step[i];

                if (currentStep != 0)
                {
                    // 計算當前單元格的邊界座標 (Chunk/Voxel 世界座標)
                    // nextBoundaryPos: 下一個單元格的邊界（根據步進方向）
                    float nextBoundaryPos = (coord[i] + (currentStep > 0 ? 1 : 0)) * size[i];

                    // tMax = (下一邊界 - 起點) / 方向
                    tMax[i] = (nextBoundaryPos - currentPosition[i]) / direction[i];

                    // 處理 tMax < 0 的情況（由於 rayOrigin 可能在邊界上，tMax 可能為 0 或極小負數）
                    // 實際上，由於 rayOrigin 已經在單元格內，tMax 應該從 0 開始計算
                    // 但為了精確，我們確保 tMax 至少為 0
                    // tMax[i] = Mathf.Max(0, tMax[i]); // 這裡可以省略，讓 DDA 自然處理
                }
                else
                {
                    // 方向為零，永遠不會到達邊界，tMax 為無限大
                    tMax[i] = Single.PositiveInfinity;
                }
            }

            const int MAX_ITERATIONS = 10000;
            int iterations = 0;
            var hitNormal = Vector3Int.zero;
            while (tCurrent < distance && iterations < MAX_ITERATIONS)
            {
                // 在循環開始時，返回當前的單元格座標
                yield return new RaycastResult()
                {
                    Coordinates = coord,
                    Distance = tCurrent,
                    HitRayNormal = hitNormal,
                };

                // 確定最短路徑的軸向 (即 tMax 最小的軸)
                if (tMax.x <= tMax.y && tMax.x <= tMax.z)
                {
                    // X 軸是最近的
                    tCurrent = tMax.x;                              // 更新累計距離參數
                    coord.x += step.x;                              // 步進到下一個單元格
                    tMax.x += tDelta.x;                             // 更新 tMax
                    hitNormal = new Vector3Int(step.x, 0, 0);       // 那面被擊中
                }
                else if (tMax.y <= tMax.z)
                {
                    // Y 軸是最近的
                    tCurrent = tMax.y;
                    coord.y += step.y;
                    tMax.y += tDelta.y;
                    hitNormal = new Vector3Int(0, step.y, 0);
                }
                else
                {
                    // Z 軸是最近的
                    tCurrent = tMax.z;
                    coord.z += step.z;
                    tMax.z += tDelta.z;
                    hitNormal = new Vector3Int(0, 0, step.z);
                }

                iterations++;
            }

            yield break;
        }
    }
}
