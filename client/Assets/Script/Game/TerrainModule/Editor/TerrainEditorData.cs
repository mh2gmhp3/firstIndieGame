using System.IO;
using UnityEditor;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class TerrainEditorData
    {
        public GameObject TerrainEditorMgrObj;
        public TerrainEditorManager TerrainEditorMgr;
        //Current Edit Runtime Data
        public TerrainEditRuntimeData CurTerrainEditRuntimeData;
        public BlockTemplateEditRuntimeData CurBlockTemplateEditRuntimeData;

        public bool LoadTerrainData(string name)
        {
            var path = Path.Combine(TerrainEditorDefine.EditTerrainDataFolderPath, name + ".asset");
            var assetData = AssetDatabase.LoadAssetAtPath<TerrainEditData>(path);
            if (assetData == null)
            {
                EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, $"無法讀取到檔案 路徑:{path}", TerrainEditorDefine.Dialog_Ok_Confirm);
                return false;
            }

            CurTerrainEditRuntimeData = new TerrainEditRuntimeData(assetData);
            TerrainEditorMgr.SetData(CurTerrainEditRuntimeData);
            return true;
        }

        public bool SaveCurTerrainData()
        {
            if (CurTerrainEditRuntimeData == null)
                return false;

            var path = Path.Combine(TerrainEditorDefine.EditTerrainDataFolderPath, CurTerrainEditRuntimeData.Name + ".asset");
            var assetData = AssetDatabase.LoadAssetAtPath<TerrainEditData>(path);
            if (assetData != null)
                AssetDatabase.DeleteAsset(path);
            var saveData = new TerrainEditData(CurTerrainEditRuntimeData);
            AssetDatabase.CreateAsset(saveData, path);
            AssetDatabase.Refresh();
            return true;
        }
    }
}
