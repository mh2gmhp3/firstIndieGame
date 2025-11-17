using System.IO;
using UnityEditor;
using UnityEngine;
using static TerrainModule.Editor.TerrainEditorManager;

namespace TerrainModule.Editor
{
    public class TerrainEditorData
    {
        public TerrainEditorWindow EditorWindow;

        public GameObject TerrainEditorMgrObj;
        public TerrainEditorManager TerrainEditorMgr;
        //Current Edit Runtime Data
        public TerrainEditRuntimeData CurTerrainEditRuntimeData;

        public BlockTemplatePreviewSetting BlockTemplatePreviewSetting;
        public BlockTemplateEditRuntimeData CurBlockTemplateEditRuntimeData;

        #region Terrain

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
            if (assetData == null)
            {
                var saveData = new TerrainEditData(CurTerrainEditRuntimeData);
                AssetDatabase.CreateAsset(saveData, path);
                AssetDatabase.Refresh();
            }
            else
            {
                assetData.UpdateData(CurTerrainEditRuntimeData);
                EditorUtility.SetDirty(assetData);
                AssetDatabase.SaveAssets();
            }

            return true;
        }

        public void ExportCurTerrainData()
        {
            if (CurTerrainEditRuntimeData == null)
                return;

            var dataName = CurTerrainEditRuntimeData.Name;
            var folderPath = TerrainDefine.GetExportFolderPath(dataName);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            Directory.CreateDirectory(folderPath);
            if (CurTerrainEditRuntimeData.BlockTemplateEditRuntimeData != null)
            {
                var materialFolderPath = folderPath + "/Material";
                Directory.CreateDirectory(materialFolderPath);
                var blockTemplateData = CurTerrainEditRuntimeData.BlockTemplateEditRuntimeData;
                var material = TerrainEditorUtility.GenTerrainMaterial(blockTemplateData.Shader, blockTemplateData.TileMap, blockTemplateData.Tiling);
                AssetDatabase.CreateAsset(material, Path.Combine(materialFolderPath, "Terrain.mat"));
            }

            var meshFolderPath = folderPath + "/Mesh";
            Directory.CreateDirectory(meshFolderPath);
            foreach (var idToChunkEditData in CurTerrainEditRuntimeData.IdToChunkEditData)
            {
                var chunkEditData = idToChunkEditData.Value;
                var chunkMesh = TerrainEditorUtility.CreateChunkMesh(CurTerrainEditRuntimeData, chunkEditData.Id);
                chunkMesh.name = $"Chunk_{chunkEditData.Id}";
                AssetDatabase.CreateAsset(chunkMesh, Path.Combine(meshFolderPath, chunkMesh.name + ".mesh"));
            }

            AssetDatabase.Refresh();
        }

        #endregion

        #region BlockTemplate

        public bool LoadBlockTemplateData(string name)
        {
            var path = Path.Combine(TerrainEditorDefine.EditBlockTemplateDataFolderPath, name + ".asset");
            var assetData = AssetDatabase.LoadAssetAtPath<BlockTemplateEditData>(path);
            if (assetData == null)
            {
                EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, $"無法讀取到檔案 路徑:{path}", TerrainEditorDefine.Dialog_Ok_Confirm);
                return false;
            }

            CurBlockTemplateEditRuntimeData = new BlockTemplateEditRuntimeData(assetData);
            TerrainEditorMgr.SetData(CurBlockTemplateEditRuntimeData, BlockTemplatePreviewSetting);
            return true;
        }

        public bool SaveCurBlockTemplateData()
        {
            if (CurBlockTemplateEditRuntimeData == null)
                return false;

            var path = Path.Combine(TerrainEditorDefine.EditBlockTemplateDataFolderPath, CurBlockTemplateEditRuntimeData.Name + ".asset");
            var assetData = AssetDatabase.LoadAssetAtPath<BlockTemplateEditData>(path);
            if (assetData == null)
            {
                var saveData = new BlockTemplateEditData(CurBlockTemplateEditRuntimeData);
                AssetDatabase.CreateAsset(saveData, path);
                AssetDatabase.Refresh();
            }
            else
            {
                assetData.UpdateData(CurBlockTemplateEditRuntimeData);
                EditorUtility.SetDirty(assetData);
                AssetDatabase.SaveAssets();
            }

            return true;
        }

        #endregion
    }
}
