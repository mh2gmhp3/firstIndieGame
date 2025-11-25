using Extension;
using Framework.Editor.Utility;
using System.Collections.Generic;
using Unity.Mathematics;
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
                    Object.DestroyImmediate(Obj);
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
                    Object.DestroyImmediate(Obj);
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

        private Transform _blockTemplateParent;
        private BlockTemplateEditRuntimeData _curBlockTemplateEditData;
        private BlockTemplatePreviewSetting _blockTemplatePreviewSetting;
        private BlockTemplatePreviewMesh _blockTemplatePreviewMesh;

        private Transform _environmentTemplateParent;
        private EnvironmentTemplateEditRuntimeData _curEnvironmentTemplateEditData;

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

            _curEditorMode = editorMode;
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

            foreach (var chunkId in _curTerrainEditData.IdToChunkEditData.Keys)
            {
                RefreshChunkMesh(chunkId);
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

        public void SetData(EnvironmentTemplateEditRuntimeData editData)
        {
            _curEnvironmentTemplateEditData = editData;
            if (_environmentTemplateParent == null)
            {
                var environmentTemplateParentGo = new GameObject("EnvironmentTemplate");
                _environmentTemplateParent = environmentTemplateParentGo.transform;
                _environmentTemplateParent.SetParent(Transform);
            }
        }

        #endregion
    }
}
