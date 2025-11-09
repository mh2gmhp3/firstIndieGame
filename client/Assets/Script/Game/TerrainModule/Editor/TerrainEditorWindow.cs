using Framework.Editor.Utility;
using System;
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
    }

    public class TerrainEditorWindow : EditorWindow
    {
        private static TerrainEditorWindow _instance;

        private EditorPageContainer _pageContainer = new EditorPageContainer(4);
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

            _pageContainer.AddPage(new CreateDataPage(_editorData));
            _pageContainer.AddPage(new LoadDataPage(_editorData));
            _pageContainer.AddPage(new EditDataPage(_editorData));
            _pageContainer.ChangeToPage(0);

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
        }

        private void OnGUI()
        {
            _pageContainer.OnGUI();
        }

        public static string[] GetEditDataFolderNames()
        {
            var editDataGUIDs = AssetDatabase.FindAssets("t:TerrainEditData", new string[] { TerrainEditorDefine.EditDataFolderPath });
            var result = new string[editDataGUIDs.Length];
            for (int i = 0; i < editDataGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(editDataGUIDs[i]);
                var name = Path.GetFileNameWithoutExtension(assetPath);
                result[i] = name;
            }
            return result;
        }
    }
}
