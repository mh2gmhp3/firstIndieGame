using UnityEditor;
using UnityEngine;

namespace Utility
{
    [CustomPropertyDrawer(typeof(MaterialIndirectField))]
    public class MaterialIndirectFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ObjectIndirectFieldUtility.OnGUI<Material>(position, property, label);
        }
    }
}
