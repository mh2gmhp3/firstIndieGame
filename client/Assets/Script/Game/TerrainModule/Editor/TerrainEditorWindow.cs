using Framework.Editor.Utility;
using GameSystem;
using GameSystem.RuntimeEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class TerrainEditorWindow : EditorWindow, IUpdateTarget
    {
        private static TerrainEditorWindow _instance;

        private EditorPageContainer _pageContainer;
        private TerrainEditorData _editorData = new TerrainEditorData();


        [MenuItem("Tools/TerrainEditor")]
        public static void OpenWindow()
        {
            _instance = GetWindow<TerrainEditorWindow>();
        }

        private void OnEnable()
        {
            Debug.Log("TerrainEditor Enable");
            EditorSceneManager.OpenScene(TerrainEditorDefine.EditorScenePath, OpenSceneMode.Single);

            _editorData.EditorWindow = this;

            if (_editorData.TerrainEditorMgr == null)
            {
                var mgrObj = new GameObject(TerrainEditorDefine.ManagerName);
                _editorData.TerrainEditorMgrObj = mgrObj;
                var monoHandler = _editorData.TerrainEditorMgrObj.AddComponent<EditorMonoBehaviorAgent>();
                monoHandler.Register(this);
                _editorData.TerrainEditorMgr = new TerrainEditorManager(mgrObj);
            }

            _pageContainer = new EditorPageContainer(3, Repaint);
            _pageContainer.AddPage(new TerrainEditMainPage(_editorData));
            _pageContainer.AddPage(new BlockTemplateEditMainPage(_editorData));
            _pageContainer.AddPage(new EnvironmentTemplateMainPage(_editorData));
            _pageContainer.ChangeToPage(0);

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorPreviewUtility.Cleanup();
        }

        private void OnGUI()
        {
            _pageContainer.OnGUI();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            _pageContainer.OnSceneGUI();
        }

        #region IUpdateTarget

        public void DoUpdate()
        {
            _editorData.TerrainEditorMgr.Update();
        }

        public void DoFixedUpdate()
        {

        }

        public void DoLateUpdate()
        {

        }

        public void DoOnGUI()
        {

        }

        public void DoDrawGizmos()
        {

        }

        #endregion
    }
}
