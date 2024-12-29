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

            if (GUILayout.Button("寫入參考至腳本內容"))
            {
                WriteRefDBToScript();
            }

            _objectReferenceDatabaseEditorGUI.OnGUI();
        }

        /// <summary>
        /// 寫入參考至腳本內容
        /// </summary>
        private void WriteRefDBToScript()
        {
            var type = _target.GetType();
            var baseType = type.BaseType;
            if (baseType == null)
                return;

            string uiTypeName = baseType.Name;
            string componentName = type.Name;

            ObjectReferenceDatabaseEditorUtility.WriteToScript(
                UIEditorDefine.GetUIScriptInitComponentPath(uiTypeName, componentName),
                _target.ObjectReferenceDb);
        }
    }
}
