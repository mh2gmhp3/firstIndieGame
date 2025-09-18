using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extension
{
    public static class AnimationCurveExtension
    {
        public static Keyframe LastKey(this AnimationCurve animationCurve)
        {
            int lastIndex = animationCurve.length - 1;
            if (lastIndex < 0)
                return default;

            return animationCurve[lastIndex];
        }
    }
}
