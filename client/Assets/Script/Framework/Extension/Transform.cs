﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extension
{
    public static class TransformExtension
    {
        public static void Reset(this Transform transform)
        {
            if (transform == null)
                return;

            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}
