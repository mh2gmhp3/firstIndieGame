using ComponentUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UIModule
{
    [CustomEditor(typeof(UIWindows))]
    public class UIWindowsEditor : Editor
    {
        private UIWindows _target = null;
        private UIWindows Target
        {
            get
            {
                if (_target == null)
                    _target = target as UIWindows;

                return _target;
            }
        }

        private GameObjectReferenceDatabaseEditorGUI _gameObjectReferenceDatabaseEditorGUI;

        public void OnEnable()
        {
            _gameObjectReferenceDatabaseEditorGUI = new GameObjectReferenceDatabaseEditorGUI(Target.GameObjectReferenceDb);
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(Target, "UIWindows");
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            _gameObjectReferenceDatabaseEditorGUI.OnGUI();
        }
    }
}
