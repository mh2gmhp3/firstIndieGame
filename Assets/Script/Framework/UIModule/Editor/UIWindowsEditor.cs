using Framework.ComponentUtility.Editor;
using UnityEditor;

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

        private ObjectReferenceDatabaseEditorGUI _objectReferenceDatabaseEditorGUI;

        public void OnEnable()
        {
            _objectReferenceDatabaseEditorGUI = new ObjectReferenceDatabaseEditorGUI(Target.ObjectReferenceDb);
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(Target, "UIWindows");
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            _objectReferenceDatabaseEditorGUI.OnGUI();
        }
    }
}
