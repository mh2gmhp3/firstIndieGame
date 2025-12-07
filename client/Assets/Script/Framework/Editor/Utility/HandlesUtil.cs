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

        public static void Sphere(Vector3 position, float radius, Color color)
        {
            var oriColor = Handles.color;
            Handles.color = color;
            Handles.SphereHandleCap(
                GUIUtility.GetControlID(FocusType.Passive),
                position,
                Quaternion.identity,
                radius * 2,
                EventType.Repaint);
            Handles.color = oriColor;
        }
    }
}
