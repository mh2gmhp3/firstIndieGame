using UnityEditor;
using UnityEngine;

namespace Utility
{
    [CustomPropertyDrawer(typeof(MeshIndirectField))]
    public class MeshIndirectFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ObjectIndirectFieldUtility.OnGUI<Mesh>(position, property, label);
        }
    }
}
