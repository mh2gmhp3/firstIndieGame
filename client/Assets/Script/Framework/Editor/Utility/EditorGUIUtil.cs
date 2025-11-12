using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Utility
{
    public static class EditorGUIUtil
    {
        public static void MinMaxSlider(string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(label);
                    minValue =
                        Mathf.Clamp(EditorGUILayout.FloatField(minValue), minLimit, maxValue);
                    maxValue =
                        Mathf.Clamp(EditorGUILayout.FloatField(maxValue), minValue, maxLimit);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.MinMaxSlider(
                    ref minValue, ref maxValue,
                    minLimit, maxLimit);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
