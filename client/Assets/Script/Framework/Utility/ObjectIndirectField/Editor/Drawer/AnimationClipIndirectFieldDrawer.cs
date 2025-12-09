using UnityEditor;
using UnityEngine;

namespace Utility
{
    [CustomPropertyDrawer(typeof(AnimationClipIndirectField))]
    public class AnimationClipIndirectFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ObjectIndirectFieldUtility.OnGUI<AnimationClip>(position, property, label);
        }
    }
}
