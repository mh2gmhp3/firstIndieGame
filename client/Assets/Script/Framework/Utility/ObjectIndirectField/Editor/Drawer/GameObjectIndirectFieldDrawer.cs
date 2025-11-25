using UnityEditor;
using UnityEngine;

namespace Utility
{
    [CustomPropertyDrawer(typeof(GameObjectIndirectField))]
    public class GameObjectIndirectFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ObjectIndirectFieldUtility.OnGUI<GameObject>(position, property, label);
        }
    }
}
