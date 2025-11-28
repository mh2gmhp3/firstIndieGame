using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
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

        public EnvironmentTemplatePreviewSetting EnvironmentTemplatePreviewSetting;
        public EnvironmentTemplateEditRuntimeData CurEnvironmentTemplateEditRuntimeData;

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

            var terrainData = new TerrainData
                (CurTerrainEditRuntimeData.BlockSize,
                CurTerrainEditRuntimeData.ChunkBlockNum,
                CurTerrainEditRuntimeData.ChunkNum);
            var idToChunkData = new Dictionary<int, ChunkData>();
            var title = "輸出地形";
            if (CurTerrainEditRuntimeData.BlockTemplateEditRuntimeData != null)
            {
                var materialFolderPath = TerrainDefine.GetExportMaterialFolderPath(dataName);
                Directory.CreateDirectory(materialFolderPath);
                var blockTemplateData = CurTerrainEditRuntimeData.BlockTemplateEditRuntimeData;
                var material = TerrainEditorUtility.GenTerrainMaterial(blockTemplateData.Shader, blockTemplateData.TileMap, blockTemplateData.Tiling);
                AssetDatabase.CreateAsset(material, TerrainDefine.GetExportMaterialPath(dataName));
                EditorUtility.DisplayProgressBar(title, "輸出地形材質", 0.1f);
            }

            var meshFolderPath = TerrainDefine.GetExportChunkMeshFolderPath(dataName);
            Directory.CreateDirectory(meshFolderPath);
            var chunkCount = CurTerrainEditRuntimeData.IdToChunkEditData.Count;
            var exportedChunkCount = 0;
            foreach (var idToChunkEditData in CurTerrainEditRuntimeData.IdToChunkEditData)
            {
                var chunkEditData = idToChunkEditData.Value;
                var chunkMesh = TerrainEditorUtility.CreateChunkMesh(CurTerrainEditRuntimeData, chunkEditData.Id);
                chunkMesh.name = $"Chunk_{chunkEditData.Id}";
                var chunkData = GetChunkData(chunkEditData.Id);
                chunkData.MeshName = chunkMesh.name;
                AssetDatabase.CreateAsset(chunkMesh, TerrainDefine.GetExportChunkMeshPath(dataName, chunkMesh.name));
                exportedChunkCount++;
                EditorUtility.DisplayProgressBar(
                    title,
                    $"輸出Chunk: {exportedChunkCount}/{chunkCount}",
                    0.1f + 0.9f * (exportedChunkCount / (float)chunkCount));
            }

            terrainData.ChunkDataList.AddRange(idToChunkData.Values);
            AssetDatabase.CreateAsset(terrainData, TerrainDefine.GetExportTerrainDataPath(CurTerrainEditRuntimeData.Name));
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(title, "輸出完成", "確認");

            #region Method

            ChunkData GetChunkData(int id)
            {
                if (!idToChunkData.TryGetValue(id, out var data))
                {
                    data = new ChunkData(id);
                    idToChunkData.Add(id, data);
                }
                return data;
            }

            #endregion
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

        #region EnviornmentTemplate

        public bool LoadEnvironmentTemplateData(string name)
        {
            var path = Path.Combine(TerrainEditorDefine.EditEnvironmentTemplateDataFolderPath, name + ".asset");
            var assetData = AssetDatabase.LoadAssetAtPath<EnvironmentTemplateEditData>(path);
            if (assetData == null)
            {
                EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, $"無法讀取到檔案 路徑:{path}", TerrainEditorDefine.Dialog_Ok_Confirm);
                return false;
            }

            CurEnvironmentTemplateEditRuntimeData = new EnvironmentTemplateEditRuntimeData(assetData);
            TerrainEditorMgr.SetData(CurEnvironmentTemplateEditRuntimeData, EnvironmentTemplatePreviewSetting);
            return true;
        }

        public bool SaveCurEnvironmentTemplateData()
        {
            if (CurEnvironmentTemplateEditRuntimeData == null)
                return false;

            var path = Path.Combine(TerrainEditorDefine.EditEnvironmentTemplateDataFolderPath, CurEnvironmentTemplateEditRuntimeData.Name + ".asset");
            var assetData = AssetDatabase.LoadAssetAtPath<EnvironmentTemplateEditData>(path);
            if (assetData == null)
            {
                var saveData = new EnvironmentTemplateEditData(CurEnvironmentTemplateEditRuntimeData);
                AssetDatabase.CreateAsset(saveData, path);
                AssetDatabase.Refresh();
            }
            else
            {
                assetData.UpdateData(CurEnvironmentTemplateEditRuntimeData);
                EditorUtility.SetDirty(assetData);
                AssetDatabase.SaveAssets();
            }

            return true;
        }

        #endregion
    }
}
