using AssetModule;
using UnityEditor;
using UnityEngine;

namespace Utility
{
    public static class ObjectIndirectFieldUtility
    {
        public static void OnGUI<T>(Rect position, SerializedProperty property, GUIContent label) where T : UnityEngine.Object
        {
            SerializedProperty guidProperty = property.FindPropertyRelative("_guid");
            SerializedProperty pathProperty = property.FindPropertyRelative("Path");
            SerializedProperty resourcePathProperty = property.FindPropertyRelative("ResourcePath");
            T currentObject = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guidProperty.stringValue));
            T newObject = EditorGUI.ObjectField(position, label, currentObject, typeof(T), false) as T;
            if (newObject != currentObject)
            {
                string path = AssetDatabase.GetAssetPath(newObject);
                pathProperty.stringValue = path;
                resourcePathProperty.stringValue = AssetPathUtility.AssetPathToResourcesPath(path);
                guidProperty.stringValue = AssetDatabase.AssetPathToGUID(path);
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
