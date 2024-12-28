using Framework.ComponentUtility.Editor;
using UnityEditor;

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
            _uiBaseEditorGUI.OnGUI();
        }
    }
}
