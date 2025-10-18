using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extension
{
    public static class FloatExtension
    {
        /// <summary>
        /// 近似相等 epsilon可設定兩值相差多少算是相等
        /// </summary>
        /// <param name="f"></param>
        /// <param name="ft"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool ApproximateEqual(this float f, float ft, float epsilon = 0.00001f)
        {
            if (f == ft)
                return true;

            return Math.Abs(f - ft) < epsilon;
        }
    }
}
