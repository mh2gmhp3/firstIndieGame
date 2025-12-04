using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

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

            //環境物件
            var envIdMapping = CurTerrainEditRuntimeData.EnvironmentTemplateIdMapping;
            if (CurTerrainEditRuntimeData.EnvironmentTemplateEditRuntimeData != null)
            {
                var envTemplateData = CurTerrainEditRuntimeData.EnvironmentTemplateEditRuntimeData;
                for (int i = 0; i < envTemplateData.CategoryDataList.Count; i++)
                {
                    var categoryData = envTemplateData.CategoryDataList[i];
                    for (int j = 0; j < categoryData.PrefabList.Count; j++)
                    {
                        var prefabData = categoryData.PrefabList[j];
                        var prefab = prefabData.Prefab.EditorInstance;
                        if (prefab == null)
                            continue;
                        if (!envIdMapping.TryGetId(false, categoryData.CategoryName, prefab.name, out var id))
                            continue;
                        terrainData.EnvironmentPrefabDataList.Add(new EnvironmentPrefabData(id, prefabData.Prefab));
                    }
                    for (int j = 0; j < categoryData.InstanceMeshDataList.Count; j++)
                    {
                        var insMeshData = categoryData.InstanceMeshDataList[j];
                        var prefab = insMeshData.OriginObject.EditorInstance;
                        if (prefab == null)
                            continue;
                        if (!envIdMapping.TryGetId(true, categoryData.CategoryName, prefab.name, out var id))
                            continue;
                        var meshData = new EnvironmentInstanceMeshData(id);
                        for (int s = 0; s < insMeshData.MeshSingleDataList.Count; s++)
                        {
                            var singleData = insMeshData.MeshSingleDataList[s];
                            meshData.MeshSingleDataList.Add(new EnvironmentInstanceMeshSingleData(singleData.Mesh, singleData.Material, singleData.Matrix));
                        }
                        for (int c = 0; c < insMeshData.ColliderDataList.Count; c++)
                        {
                            var colliderData = insMeshData.ColliderDataList[i];
                            meshData.ColliderDataList.Add(new EnvironmentColliderData(
                                colliderData.ColliderType,
                                colliderData.Center,
                                colliderData.Size,
                                colliderData.Radius,
                                colliderData.Height,
                                colliderData.Direction,
                                colliderData.Position,
                                colliderData.Rotation,
                                colliderData.Scale));
                        }
                        terrainData.EnvironmentInstanceMeshDataList.Add(meshData);
                    }
                }
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
                AssetDatabase.CreateAsset(chunkMesh, TerrainDefine.GetExportChunkMeshPath(dataName, chunkMesh.name));
                chunkData.Mesh.EditorInstance = chunkMesh;
                for (int i = 0; i < chunkEditData.EnvironmentEditDataList.Count; i++)
                {
                    var envEditData = chunkEditData.EnvironmentEditDataList[i];
                    if (!envIdMapping.TryGetId(envEditData.IsInstanceMesh, envEditData.CategoryName, envEditData.Name, out var id))
                        continue;
                    var envData = new ChunkEnvironmentData(envEditData.IsInstanceMesh, id);
                    for (int j = 0; j < envEditData.InstanceList.Count; j++)
                    {
                        var insEditData = envEditData.InstanceList[j];
                        envData.InstanceList.Add(new ChunkEnvironmentInstanceData(
                            insEditData.InstanceId,
                            insEditData.Position,
                            insEditData.Rotation,
                            insEditData.Scale));
                    }
                    chunkData.EnvironmentDataList.Add(envData);
                }
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
