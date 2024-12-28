using Framework.ComponentUtility.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace UIModule
{
    public class UIBaseEditorGUI
    {
        private UIBase _target = null;

        private ObjectReferenceDatabaseEditorGUI _objectReferenceDatabaseEditorGUI;

        private SerializedObject _targetSerializedObject = null;

        public UIBaseEditorGUI(UIBase target)
        {
            _target = target;
            _targetSerializedObject = new SerializedObject(_target);
            _objectReferenceDatabaseEditorGUI = new ObjectReferenceDatabaseEditorGUI(_target.ObjectReferenceDb);
        }

        public void OnGUI()
        {
            Undo.RecordObject(_targetSerializedObject.targetObject, "UIBase");
            _targetSerializedObject.ApplyModifiedProperties();
            _targetSerializedObject.Update();

            _objectReferenceDatabaseEditorGUI.OnGUI();
        }
    }
}
