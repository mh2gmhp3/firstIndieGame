using Extension;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class TerrainEditorManager
    {
        public enum EditorMode
        {
            Terrain,
            BlockTemplate,
            EnvironmentTemplate
        }

        private class ChunkPreviewMesh
        {
            public GameObject Obj;
            public MeshFilter MeshFilter;
            public MeshRenderer MeshRenderer;
            public Mesh Mesh;

            public ChunkPreviewMesh(int id, Transform parent)
            {
                Obj = new GameObject($"Chunk_{id}");
                Obj.transform.SetParent(parent);
                MeshFilter = Obj.AddComponent<MeshFilter>();
                MeshRenderer = Obj.AddComponent<MeshRenderer>();
                Mesh = new Mesh();
                Mesh.name = $"Chunk_{id}";
                Mesh.MarkDynamic();
                MeshFilter.mesh = Mesh;
            }

            public void Clear()
            {
                if (Obj != null)
                    UnityEngine.Object.DestroyImmediate(Obj);
                MeshFilter = null;
                Mesh = null;
            }
        }

        private class BlockTemplatePreviewMesh
        {
            public GameObject Obj;
            public MeshFilter MeshFilter;
            public MeshRenderer MeshRenderer;
            public Mesh Mesh;

            public BlockTemplatePreviewMesh(Transform parent)
            {
                Obj = new GameObject($"BlockTemplatePreview");
                Obj.transform.SetParent(parent);
                MeshFilter = Obj.AddComponent<MeshFilter>();
                MeshRenderer = Obj.AddComponent<MeshRenderer>();
                Mesh = new Mesh();
                Mesh.name = $"BlockTemplatePreview";
                Mesh.MarkDynamic();
                MeshFilter.mesh = Mesh;
            }

            public void Clear()
            {
                if (Obj != null)
                    UnityEngine.Object.DestroyImmediate(Obj);
                MeshFilter = null;
                Mesh = null;
            }
        }

        private GameObject GameObject;
        private Transform Transform;

        private EditorMode _curEditorMode = EditorMode.Terrain;

        private Transform _terrainChunkParent;
        private TerrainEditRuntimeData _curTerrainEditData;
        private Dictionary<int, ChunkPreviewMesh> _chunkIdToPreviewMeshDic = new Dictionary<int, ChunkPreviewMesh>();
        private Transform _terrainEnvironmentDrawTrans;
        private MeshBatchController _terrainEnvironmentDrawInsMesh = new MeshBatchController();
        private TerrainEnvironmentController _terrainEnvironmentController = new TerrainEnvironmentController();

        private Transform _blockTemplateParent;
        private BlockTemplateEditRuntimeData _curBlockTemplateEditData;
        private BlockTemplatePreviewSetting _blockTemplatePreviewSetting;
        private BlockTemplatePreviewMesh _blockTemplatePreviewMesh;

        private Transform _environmentTemplateParent;
        private EnvironmentTemplateEditRuntimeData _curEnvironmentTemplateEditData;
        private EnvironmentTemplatePreviewSetting _environmentTemplatePreviewSetting;
        private GameObject _environmentPrefabPreviewObj;
        private MeshBatchController _environmentIneMeshPreviewController = new MeshBatchController();

        public TerrainEditorManager(GameObject sceneObj)
        {
            GameObject = sceneObj;
            Transform = GameObject.transform;
        }

        public void ChangeMode(EditorMode editorMode)
        {
            if (_curEditorMode == editorMode)
                return;

            //Exit
            if (_curEditorMode == EditorMode.Terrain)
            {
                if (_terrainChunkParent != null)
                    _terrainChunkParent.gameObject.SetActive(false);
            }
            else if (_curEditorMode == EditorMode.BlockTemplate)
            {
                if (_blockTemplateParent != null)
                    _blockTemplateParent.gameObject.SetActive(false);
            }
            else if (_curEditorMode == EditorMode.EnvironmentTemplate)
            {
                if (_environmentTemplateParent != null)
                    _environmentTemplateParent.gameObject.SetActive(false);
                _environmentIneMeshPreviewController.Clear();
            }

            //Enter
            if (editorMode == EditorMode.Terrain)
            {
                if (_terrainChunkParent != null)
                {
                    _terrainChunkParent.gameObject.SetActive(true);
                    RefreshBlockTemplatePreview();
                }
            }
            else if (editorMode == EditorMode.BlockTemplate)
            {
                if (_blockTemplateParent != null)
                {
                    _blockTemplateParent.gameObject.SetActive(true);
                    RefreshBlockTemplatePreview();
                }
            }
            else if (editorMode == EditorMode.EnvironmentTemplate)
            {
                if (_environmentTemplateParent != null)
                {
                    _environmentTemplateParent.gameObject.SetActive(true);
                    RefreshEnvironmentTemplatePreview();
                }
            }

            _curEditorMode = editorMode;
        }

        public void Update()
        {
            UpdateTerrainEnvironmentDrawInsMesh();
            UpdateTerrainEnvironmentInsMesh();
            EnvironmentTemplateUpdateInsMesh();
        }

        #region Terrain

        public void SetData(TerrainEditRuntimeData editData)
        {
            _curTerrainEditData = editData;
            if (_terrainChunkParent == null)
            {
                var terrainChunkParentGo = new GameObject("TerrainChunk");
                _terrainChunkParent = terrainChunkParentGo.transform;
                _terrainChunkParent.SetParent(Transform);
            }

            _terrainEnvironmentController.SetGetAction(GetEnvironmentPrefabInfo, GetEnvironmentMeshInfo);
            RebuildAllTerrainPreviewMesh();
        }

        public void RebuildAllTerrainPreviewMesh()
        {
            if (_curEditorMode != EditorMode.Terrain)
                return;

            if (_curTerrainEditData == null)
                return;

            //Clear Origin
            foreach (var previewMesh in _chunkIdToPreviewMeshDic.Values)
            {
                previewMesh.Clear();
            }
            _chunkIdToPreviewMeshDic.Clear();

            _terrainEnvironmentController.Clear();

            foreach (var chunkId in _curTerrainEditData.IdToChunkEditData.Keys)
            {
                RefreshChunkMesh(chunkId);
                RefreshChunkEnvironment(chunkId);
            }
        }

        public void RefreshChunkMesh(int chunkId)
        {
            if (_curEditorMode != EditorMode.Terrain)
                return;

            if (_curTerrainEditData == null)
                return;

            if (!_chunkIdToPreviewMeshDic.TryGetValue(chunkId, out var previewMesh))
            {
                previewMesh = new ChunkPreviewMesh(chunkId, _terrainChunkParent);
                _chunkIdToPreviewMeshDic.Add(chunkId, previewMesh);
            }
            TerrainEditorUtility.CreateChunkMesh(previewMesh.Mesh, _curTerrainEditData, chunkId);
            previewMesh.MeshRenderer.sharedMaterial = _curTerrainEditData.TerrainMaterial;
        }

        public void SetTerrainEnvironmentDraw(EnvironmentTemplatePreviewSetting drawPreviewSetting)
        {
            if (_curEditorMode != EditorMode.Terrain)
                return;

            if (drawPreviewSetting == null)
                return;

            if (_curTerrainEditData == null)
                return;

            if (!_curTerrainEditData.EnvironmentTemplateEditRuntimeData.TryGetCategory(drawPreviewSetting.CategoryName, out var categoryData))
                return;

            ClearTerrainEnvironmentDraw();

            if (drawPreviewSetting.IsInstanceMesh)
            {
                if (categoryData.InstanceMeshDataList.TryGet(drawPreviewSetting.Index, out var instanceMeshData))
                {
                    _terrainEnvironmentDrawInsMesh.Clear();
                    for (int i = 0; i < instanceMeshData.MeshSingleDataList.Count; i++)
                    {
                        var singleData = instanceMeshData.MeshSingleDataList[i];
                        var mesh = singleData.Mesh.EditorInstance;
                        var material = singleData.Material.EditorInstance;
                        var matrix = singleData.Matrix;
                        _terrainEnvironmentDrawInsMesh.AddMeshBatchData(mesh, material, matrix);
                    }
                    _terrainEnvironmentDrawInsMesh.AddInstance(0, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));
                }
            }
            else
            {
                if (categoryData.PrefabList.TryGet(drawPreviewSetting.Index, out var prefabData))
                {
                    if (prefabData.Prefab.EditorInstance == null)
                        return;
                    var go = UnityEngine.Object.Instantiate(prefabData.Prefab.EditorInstance);
                    go.transform.SetParent(_terrainChunkParent);
                    go.transform.Reset();
                    _terrainEnvironmentDrawTrans = go.transform;
                }
            }
        }

        public void UpdateTerrainEnvironmentDraw(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (_curEditorMode != EditorMode.Terrain)
                return;

            if (_terrainEnvironmentDrawTrans != null)
            {
                _terrainEnvironmentDrawTrans.position = position;
                _terrainEnvironmentDrawTrans.rotation = rotation;
                _terrainEnvironmentDrawTrans.localScale = scale;
            }
            _terrainEnvironmentDrawInsMesh.UpdateInstance(0, Matrix4x4.TRS(position, rotation, scale));
        }

        public void ClearTerrainEnvironmentDraw()
        {
            if (_terrainEnvironmentDrawTrans != null)
                UnityEngine.Object.DestroyImmediate(_terrainEnvironmentDrawTrans.gameObject);

            _terrainEnvironmentDrawInsMesh.Clear();
            _terrainEnvironmentDrawInsMesh.UpdateInstance();
        }

        private void UpdateTerrainEnvironmentDrawInsMesh()
        {
            if (_curEditorMode != EditorMode.Terrain)
                return;
            _terrainEnvironmentDrawInsMesh.UpdateInstance();
        }

        public void RefreshChunkEnvironment(int chunkId)
        {
            if (_curEditorMode != EditorMode.Terrain)
                return;

            if (_curTerrainEditData == null)
                return;

            if (!_curTerrainEditData.IdToChunkEditData.TryGetValue(chunkId, out var chunkData))
                return;

            for (int i = 0; i < chunkData.EnvironmentEditDataList.Count; i++)
            {
                var envData = chunkData.EnvironmentEditDataList[i];
                if (!_curTerrainEditData.EnvironmentTemplateIdMapping.TryGetId(envData.IsInstanceMesh, envData.CategoryName, envData.Name, out var id))
                    continue;
                for (int j = 0; j < envData.InstanceList.Count; j++)
                {
                    var instanceData = envData.InstanceList[j];
                    if (envData.IsInstanceMesh)
                    {
                        _terrainEnvironmentController.AddMeshInstance(
                            id,
                            instanceData.InstanceId,
                            instanceData.Position,
                            instanceData.Rotation,
                            instanceData.Scale);
                    }
                    else
                    {
                        _terrainEnvironmentController.AddPrefabInstance(
                            id,
                            instanceData.InstanceId,
                            instanceData.Position,
                            instanceData.Rotation,
                            instanceData.Scale);
                    }
                }
            }
        }

        public void RemoveEnvironmentInstance(List<int> removeInsIdList)
        {
            if (_curEditorMode != EditorMode.Terrain)
                return;

            if (_curTerrainEditData == null)
                return;

            for (int i = 0; i < removeInsIdList.Count; i++)
            {
                var instanceId = removeInsIdList[i];
                _terrainEnvironmentController.RemoveInstance(instanceId);
            }
        }

        private void UpdateTerrainEnvironmentInsMesh()
        {
            if (_curEditorMode != EditorMode.Terrain)
                return;
            _terrainEnvironmentController.Update();
        }

        private TerrainEnvironmentController.PrefabInfo GetEnvironmentPrefabInfo(int id)
        {
            if (!_curTerrainEditData.TryGetEnvPrefabData(id, out var prefabData))
                return default;

            return new TerrainEnvironmentController.PrefabInfo()
            {
                Prefab = prefabData.Prefab.EditorInstance,
                Parent = _terrainChunkParent
            };
        }

        private List<TerrainEnvironmentController.MeshSingleInfo> _meshSingleInfoCache = new List<TerrainEnvironmentController.MeshSingleInfo>();
        private TerrainEnvironmentController.MeshInfo GetEnvironmentMeshInfo(int id)
        {
            if (!_curTerrainEditData.TryGetEnvInsMeshData(id, out var insMeshData))
                return default;

            _meshSingleInfoCache.Clear();
            for (int i = 0; i < insMeshData.MeshSingleDataList.Count; i++)
            {
                var meshData = insMeshData.MeshSingleDataList[i];
                _meshSingleInfoCache.Add(new TerrainEnvironmentController.MeshSingleInfo()
                {
                    Mesh = meshData.Mesh.EditorInstance,
                    Material = meshData.Material.EditorInstance,
                    Matrix = meshData.Matrix
                });
            }
            return new TerrainEnvironmentController.MeshInfo()
            {
                MeshSingleInfoList = _meshSingleInfoCache
            };
        }

        #endregion

        #region BlockTemplate

        public void SetData(BlockTemplateEditRuntimeData editData, BlockTemplatePreviewSetting previewSetting)
        {
            _curBlockTemplateEditData = editData;
            _blockTemplatePreviewSetting = previewSetting;
            if (_blockTemplateParent == null)
            {
                var blockTemplateParentGo = new GameObject("BlockTemplate");
                _blockTemplateParent = blockTemplateParentGo.transform;
                _blockTemplateParent.SetParent(Transform);
            }

            RefreshBlockTemplatePreview();
        }

        public void RefreshBlockTemplatePreview()
        {
            if (_curEditorMode != EditorMode.BlockTemplate)
                return;

            if (_curBlockTemplateEditData == null)
                return;

            if (_blockTemplatePreviewSetting == null)
                return;

            if (!_curBlockTemplateEditData.TryGetBlockData(_blockTemplatePreviewSetting.PreviewId, out var previewBlockData))
                return;

            if (_blockTemplatePreviewMesh == null)
            {
                _blockTemplatePreviewMesh = new BlockTemplatePreviewMesh(_blockTemplateParent);
            }

            TerrainEditorUtility.CreatePreviewBlockMesh(
                _blockTemplatePreviewMesh.Mesh,
                previewBlockData,
                _blockTemplatePreviewSetting,
                Vector3.zero);
            _blockTemplatePreviewMesh.MeshRenderer.sharedMaterial = _curBlockTemplateEditData.Material;
        }

        #endregion

        #region EnvironmentTemplate

        public void SetData(EnvironmentTemplateEditRuntimeData editData, EnvironmentTemplatePreviewSetting previewSetting)
        {
            _curEnvironmentTemplateEditData = editData;
            _environmentTemplatePreviewSetting = previewSetting;
            if (_environmentTemplateParent == null)
            {
                var environmentTemplateParentGo = new GameObject("EnvironmentTemplate");
                _environmentTemplateParent = environmentTemplateParentGo.transform;
                _environmentTemplateParent.SetParent(Transform);
            }
        }

        public void RefreshEnvironmentTemplatePreview()
        {
            if (_curEditorMode != EditorMode.EnvironmentTemplate)
                return;

            if (_curEnvironmentTemplateEditData == null)
                return;

            if (_environmentTemplatePreviewSetting == null)
                return;

            if (_environmentPrefabPreviewObj != null)
                UnityEngine.Object.DestroyImmediate(_environmentPrefabPreviewObj);

            if (!_curEnvironmentTemplateEditData.TryGetCategory(_environmentTemplatePreviewSetting.CategoryName, out var categoryEditRuntimeData))
                return;

            if (_environmentTemplatePreviewSetting.IsInstanceMesh)
            {
                if (categoryEditRuntimeData.InstanceMeshDataList.TryGet(_environmentTemplatePreviewSetting.Index, out var instanceMeshData))
                {
                    _environmentIneMeshPreviewController.Clear();
                    for (int i = 0; i < instanceMeshData.MeshSingleDataList.Count; i++)
                    {
                        var singleData = instanceMeshData.MeshSingleDataList[i];
                        var mesh = singleData.Mesh.EditorInstance;
                        var material = singleData.Material.EditorInstance;
                        var matrix = singleData.Matrix;
                        _environmentIneMeshPreviewController.AddMeshBatchData(mesh, material, matrix);
                    }
                    _environmentIneMeshPreviewController.AddInstance(0, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));
                }
            }
            else
            {
                if (categoryEditRuntimeData.PrefabList.TryGet(_environmentTemplatePreviewSetting.Index, out var prefabData))
                {
                    if (prefabData.Prefab.EditorInstance == null)
                        return;
                    var go = UnityEngine.Object.Instantiate(prefabData.Prefab.EditorInstance);
                    go.transform.SetParent(_environmentTemplateParent);
                    go.transform.Reset();
                    _environmentPrefabPreviewObj = go;
                }
            }
        }

        private void EnvironmentTemplateUpdateInsMesh()
        {
            if (_curEditorMode != EditorMode.EnvironmentTemplate)
                return;

            if (!_environmentTemplatePreviewSetting.IsInstanceMesh)
                return;

            _environmentIneMeshPreviewController.UpdateInstance();
        }

        #endregion
    }
}
