using Framework.ComponentUtility.Editor;
using UnityEditor;
using UnityEngine;

namespace UIModule
{
    [CustomEditor(typeof(UIWindow), true)]
    public class UIWindowsEditor : Editor
    {
        private UIWindow _target = null;
        private UIWindow Target
        {
            get
            {
                if (_target == null)
                    _target = target as UIWindow;

                return _target;
            }
        }

        private UIBaseEditorGUI _uiBaseEditorGUI;

        public void OnEnable()
        {
            _uiBaseEditorGUI = new UIBaseEditorGUI(Target);
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("開啟編輯視窗"))
                UIEditorWindow.OpenWindow(Target.gameObject);
            _uiBaseEditorGUI.OnGUI();
        }
    }
}
