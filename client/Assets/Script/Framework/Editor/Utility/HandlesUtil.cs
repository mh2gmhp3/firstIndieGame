using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Utility
{
    public static class HandlesUtil
    {
        public static Vector3 SphereSlider(Vector3 position, Vector3 direction, float size)
        {
            return Handles.Slider(position, direction, size, Handles.SphereHandleCap, -1);
        }
    }
}
