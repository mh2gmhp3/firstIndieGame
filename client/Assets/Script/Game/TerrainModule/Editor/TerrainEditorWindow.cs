using Framework.Editor.Utility;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class TerrainEditorData
    {
        public GameObject TerrainEditorMgrObj;
        public TerrainEditorManager TerrainEditorMgr;
        //Current Edit Runtime Data
        public TerrainEditRuntimeData CurEditRuntimeData;

        public bool LoadData(string name)
        {
            var path = Path.Combine(TerrainEditorDefine.EditDataFolderPath, name + ".asset");
            var assetData = AssetDatabase.LoadAssetAtPath<TerrainEditData>(path);
            if (assetData == null)
            {
                EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, $"無法讀取到檔案 路徑:{path}", TerrainEditorDefine.Dialog_Ok_Confirm);
                return false;
            }

            CurEditRuntimeData = new TerrainEditRuntimeData(assetData);
            TerrainEditorMgr.Init(CurEditRuntimeData);
            return true;
        }

        public bool SaveCurData()
        {
            if (CurEditRuntimeData == null)
                return false;

            var path = Path.Combine(TerrainEditorDefine.EditDataFolderPath, CurEditRuntimeData.Name + ".asset");
            var assetData = AssetDatabase.LoadAssetAtPath<TerrainEditData>(path);
            if (assetData != null)
                AssetDatabase.DeleteAsset(path);
            var saveData = new TerrainEditData(CurEditRuntimeData);
            AssetDatabase.CreateAsset(saveData, path);
            AssetDatabase.Refresh();
            return true;
        }
    }

    public class TerrainEditorWindow : EditorWindow
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

            var mgr = FindObjectOfType<TerrainEditorManager>();
            if (mgr == null)
            {
                var mgrObj = new GameObject(TerrainEditorDefine.ManagerName);
                _editorData.TerrainEditorMgrObj = mgrObj;
                _editorData.TerrainEditorMgr = mgrObj.AddComponent<TerrainEditorManager>();
            }
            else
            {
                _editorData.TerrainEditorMgrObj = mgr.gameObject;
                _editorData.TerrainEditorMgr = mgr;
            }

            _pageContainer = new EditorPageContainer(4, Repaint);
            _pageContainer.AddPage(new CreateDataPage(_editorData));
            _pageContainer.AddPage(new LoadDataPage(_editorData));
            _pageContainer.AddPage(new EditDataPage(_editorData));
            _pageContainer.ChangeToPage(0);

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            _pageContainer.OnGUI();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            _pageContainer.OnSceneGUI();
        }
    }
}
