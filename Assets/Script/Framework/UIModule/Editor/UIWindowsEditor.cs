using Framework.ComponentUtility.Editor;
using UnityEditor;

namespace UIModule
{
    [CustomEditor(typeof(UIWindow))]
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

        private ObjectReferenceDatabaseEditorGUI _objectReferenceDatabaseEditorGUI;

        public void OnEnable()
        {
            _objectReferenceDatabaseEditorGUI = new ObjectReferenceDatabaseEditorGUI(Target.ObjectReferenceDb);
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(Target, "UIWindow");
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            _objectReferenceDatabaseEditorGUI.OnGUI();
        }
    }
}
