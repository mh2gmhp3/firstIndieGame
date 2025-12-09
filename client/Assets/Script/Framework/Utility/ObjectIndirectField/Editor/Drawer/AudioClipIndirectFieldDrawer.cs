using UnityEditor;
using UnityEngine;

namespace Utility
{
    [CustomPropertyDrawer(typeof(AudioClipIndirectField))]
    public class AudioClipIndirectFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ObjectIndirectFieldUtility.OnGUI<AudioClip>(position, property, label);
        }
    }
}
