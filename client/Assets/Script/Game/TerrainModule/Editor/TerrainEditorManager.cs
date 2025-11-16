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
            BlockTemplate
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

        public class BlockTemplatePreviewSetting
        {
            public Vector3Int BlockSize = Vector3Int.one;
            public Vector4 YTopValue = Vector4.one;
            public Vector4 YBottomValue = Vector4.zero;

            public float PXRotation;
            public float NXRotation;

            public float PYRotation;
            public float NYRotation;

            public float PZRotation;
            public float NZRotation;

            public int PreviewId;

            public void SetYValue(Vector4 topValue, Vector4 bottomValue)
            {
                var clampValue = TerrainEditorUtility.ClampYValue(topValue, bottomValue);
                YTopValue = clampValue.TopValue;
                YBottomValue = clampValue.BottomValue;
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
            previewMesh.Mesh.Clear();

            if (!_curTerrainEditData.IdToChunkEditData.TryGetValue(chunkId, out var chunkEditData))
                return;

            var mesh = previewMesh.Mesh;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var uvs2 = new List<Vector2>();
            var uvs3 = new List<Vector2>();
            foreach (var blockEditDataPair in chunkEditData.IdToBlockEditData)
            {
                var blockEditData = blockEditDataPair.Value;
                CreateBlock(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    chunkEditData,
                    blockEditData);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.uv2 = uvs2.ToArray();
            mesh.uv3 = uvs3.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            previewMesh.MeshRenderer.sharedMaterial = _curTerrainEditData.TerrainMaterial;
        }

        private void CreateBlock(
            List<Vector3> vertices,
            List<int> triangles,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Vector2> uvs2,
            List<Vector2> uvs3,
            ChunkEditRuntimeData chunkEditRuntimeData,
            BlockEditRuntimeData blockEditRuntimeData)
        {
            var chunkId = chunkEditRuntimeData.Id;
            var blockId = blockEditRuntimeData.Id;
            var BlockTemplateId = blockEditRuntimeData.TemplateId;
            var size = _curTerrainEditData.BlockSize;
            var worldBlockCoord = _curTerrainEditData.GetWorldBlockCoordWithId(chunkId, blockId);
            var worldBlockPivotPos = worldBlockCoord * size;

            BlockEditRuntimeData refBlockData = null;

            bool createFace = false;
            Vector4 yTopValue = blockEditRuntimeData.YTopValue;
            Vector4 yBottomValue = blockEditRuntimeData.YBottomValue;

            if (!_curTerrainEditData.BlockTemplateEditRuntimeData.TryGetBlockData(BlockTemplateId, out var blockTemplateData))
            {
                blockTemplateData = new BlockTemplateRuntimeData(0);
                Debug.Log($"ChunkId:{chunkId} BlockId:{blockId} BlockTemplateId:{BlockTemplateId} 找不到方塊範本, 使用預設");
            }

            //方向沒方塊必須建立面
            //有方塊必須完全共面才不需建立 不考慮交錯要補面問題
            // +x
            var worldRefBlockCoord = worldBlockCoord + Vector3Int.right;
            if (_curTerrainEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = !(blockEditRuntimeData.YTopValue.y.ApproximateEqual(refBlockData.YTopValue.x)
                    && blockEditRuntimeData.YBottomValue.y.ApproximateEqual(refBlockData.YBottomValue.x)
                    && blockEditRuntimeData.YTopValue.w.ApproximateEqual(refBlockData.YTopValue.z)
                    && blockEditRuntimeData.YBottomValue.w.ApproximateEqual(refBlockData.YBottomValue.z));
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.right,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.PXTiling,
                    blockEditRuntimeData.PXRotation);
            }

            // -x
            worldRefBlockCoord = worldBlockCoord + Vector3Int.left;
            if (_curTerrainEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = !(blockEditRuntimeData.YTopValue.x.ApproximateEqual(refBlockData.YTopValue.y)
                    && blockEditRuntimeData.YBottomValue.x.ApproximateEqual(refBlockData.YBottomValue.y)
                    && blockEditRuntimeData.YTopValue.z.ApproximateEqual(refBlockData.YTopValue.w)
                    && blockEditRuntimeData.YBottomValue.z.ApproximateEqual(refBlockData.YBottomValue.w));
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.left,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.NXTiling,
                    blockEditRuntimeData.NXRotation);
            }

            // +z
            worldRefBlockCoord = worldBlockCoord + Vector3Int.forward;
            if (_curTerrainEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = !(blockEditRuntimeData.YTopValue.z.ApproximateEqual(refBlockData.YTopValue.x)
                    && blockEditRuntimeData.YBottomValue.z.ApproximateEqual(refBlockData.YBottomValue.x)
                    && blockEditRuntimeData.YTopValue.w.ApproximateEqual(refBlockData.YTopValue.y)
                    && blockEditRuntimeData.YBottomValue.w.ApproximateEqual(refBlockData.YBottomValue.y));
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.forward,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.PZTiling,
                    blockEditRuntimeData.PZRotation);
            }

            // -z
            worldRefBlockCoord = worldBlockCoord + Vector3Int.back;
            if (_curTerrainEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = !(blockEditRuntimeData.YTopValue.x.ApproximateEqual(refBlockData.YTopValue.z)
                    && blockEditRuntimeData.YBottomValue.x.ApproximateEqual(refBlockData.YBottomValue.z)
                    && blockEditRuntimeData.YTopValue.y.ApproximateEqual(refBlockData.YTopValue.w)
                    && blockEditRuntimeData.YBottomValue.y.ApproximateEqual(refBlockData.YBottomValue.w));
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.back,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.NZTiling,
                    blockEditRuntimeData.NZRotation);
            }

            // +y
            worldRefBlockCoord = worldBlockCoord + Vector3Int.up;
            if (_curTerrainEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = blockEditRuntimeData.YTopValue != Vector4.one || refBlockData.YBottomValue != Vector4.zero;
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.up,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.PYTiling,
                    blockEditRuntimeData.PYRotation);
            }

            // -y
            worldRefBlockCoord = worldBlockCoord + Vector3Int.down;
            if (_curTerrainEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = blockEditRuntimeData.YBottomValue != Vector4.zero || refBlockData.YTopValue != Vector4.one;
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.down,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.NYTiling,
                    blockEditRuntimeData.NYRotation);
            }
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

            var size = _blockTemplatePreviewSetting.BlockSize;
            var worldBlockPivotPos = Vector3.zero;

            Vector4 yTopValue = _blockTemplatePreviewSetting.YTopValue;
            Vector4 yBottomValue = _blockTemplatePreviewSetting.YBottomValue;

            if (_blockTemplatePreviewMesh == null)
            {
                _blockTemplatePreviewMesh = new BlockTemplatePreviewMesh(_blockTemplateParent);
            }
            _blockTemplatePreviewMesh.Mesh.Clear();

            var mesh = _blockTemplatePreviewMesh.Mesh;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var uvs2 = new List<Vector2>();
            var uvs3 = new List<Vector2>();

            // +x
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.right,
                yTopValue,
                yBottomValue,
                previewBlockData.PXTiling,
                _blockTemplatePreviewSetting.PXRotation);

            // -x
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.left,
                yTopValue,
                yBottomValue,
                previewBlockData.NXTiling,
                _blockTemplatePreviewSetting.NXRotation);

            // +z
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.forward,
                yTopValue,
                yBottomValue,
                previewBlockData.PZTiling,
                _blockTemplatePreviewSetting.PZRotation);

            // -z
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.back,
                yTopValue,
                yBottomValue,
                previewBlockData.NZTiling,
                _blockTemplatePreviewSetting.NZRotation);

            // +y
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.up,
                yTopValue,
                yBottomValue,
                previewBlockData.PYTiling,
                _blockTemplatePreviewSetting.PYRotation);

            // -y
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.down,
                yTopValue,
                yBottomValue,
                previewBlockData.NYTiling,
                _blockTemplatePreviewSetting.NYRotation);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.uv2 = uvs2.ToArray();
            mesh.uv3 = uvs3.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            _blockTemplatePreviewMesh.MeshRenderer.sharedMaterial = _curBlockTemplateEditData.Material;
        }

        #endregion

        #region Face

        private void CreateBlockFace(
            List<Vector3> vertices,
            List<int> triangles,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Vector2> uvs2,
            List<Vector2> uvs3,
            Vector3 worldPosition,
            Vector3 size,
            Vector3Int faceNormal,
            Vector4 yTopValue,
            Vector4 yBottomValue,
            Vector2 tiling,
            float rotationValue)
        {
            Vector4 topY = size.y * yTopValue;
            Vector4 bottomY = size.y * yBottomValue;

            Vector3[] addVertices = null;
            int[] addTriangles = null;
            Vector2[] addUVs = null;

            //+x
            if (faceNormal == Vector3Int.right)
            {
                var p0Pos = worldPosition + new Vector3(size.x, 0, 0);
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.y, 0),                                   // Bottom-left
                    p0Pos + new Vector3(0, bottomY.w, size.z),                              // Bottom-right
                    p0Pos + new Vector3(0, topY.y, 0),                                      // Top-left
                    p0Pos + new Vector3(0, topY.w, size.z)                                  // Top-right
                };
                addTriangles = GetTriangle(vertices.Count, false);
                addUVs = new Vector2[]
                {
                    new Vector2(0, yBottomValue.y),
                    new Vector2(1 , yBottomValue.w),
                    new Vector2(0, yTopValue.y),
                    new Vector2(1, yTopValue.w)
                };
            }
            //-x
            else if (faceNormal == Vector3Int.left)
            {
                var p0Pos = worldPosition + new Vector3(0, 0, size.z);
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.z, 0),                                    // Bottom-left
                    p0Pos + new Vector3(0, bottomY.x, -size.z),                              // Bottom-right
                    p0Pos + new Vector3(0, topY.z, 0),                                       // Top-left
                    p0Pos + new Vector3(0, topY.x, -size.z)                                  // Top-right
                };
                addTriangles = GetTriangle(vertices.Count, false);
                addUVs = new Vector2[]
                {
                    new Vector2(0, yBottomValue.z),
                    new Vector2(1 , yBottomValue.x),
                    new Vector2(0, yTopValue.z),
                    new Vector2(1, yTopValue.x)
                };
            }
            //+z
            else if (faceNormal == Vector3Int.forward)
            {
                var p0Pos = worldPosition + new Vector3(size.x, 0, size.z);
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.w, 0),                                    // Bottom-left
                    p0Pos + new Vector3(-size.x, bottomY.z, 0),                              // Bottom-right
                    p0Pos + new Vector3(0, topY.w, 0) ,                                      // Top-left
                    p0Pos + new Vector3(-size.x, topY.z, 0)                                  // Top-right
                };
                addTriangles = GetTriangle(vertices.Count, false);
                addUVs = new Vector2[]
                {
                    new Vector2(0, yBottomValue.w),
                    new Vector2(1 , yBottomValue.z),
                    new Vector2(0, yTopValue.w),
                    new Vector2(1, yTopValue.z)
                };
            }
            //-z
            else if (faceNormal == Vector3Int.back)
            {
                var p0Pos = worldPosition;
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.x, 0),                                    // Bottom-left
                    p0Pos + new Vector3(size.x, bottomY.y, 0),                               // Bottom-right
                    p0Pos + new Vector3(0, topY.x, 0) ,                                      // Top-left
                    p0Pos + new Vector3(size.x, topY.y, 0)                                   // Top-right
                };
                addTriangles = GetTriangle(vertices.Count, false);
                addUVs = new Vector2[]
                {
                    new Vector2(0, yBottomValue.x),
                    new Vector2(1 , yBottomValue.y),
                    new Vector2(0, yTopValue.x),
                    new Vector2(1, yTopValue.y)
                };
            }
            //+y
            else if (faceNormal == Vector3Int.up)
            {
                var p0Pos = worldPosition;
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, topY.x, 0),                                    // Bottom-left
                    p0Pos + new Vector3(size.x, topY.y, 0),                               // Bottom-right
                    p0Pos + new Vector3(0, topY.z, size.z) ,                              // Top-left
                    p0Pos + new Vector3(size.x, topY.w, size.z)                           // Top-right
                };
                bool mirrorTriangle =
                    yTopValue.x > yTopValue.y ||
                    yTopValue.w > yTopValue.z;
                addTriangles = GetTriangle(vertices.Count, mirrorTriangle);
                addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
            }
            //-y
            else if (faceNormal == Vector3Int.down)
            {
                var p0Pos = worldPosition + new Vector3(0, 0, size.z);
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.z, 0),                                     // Bottom-left
                    p0Pos + new Vector3(size.x, bottomY.w, 0),         // Bottom-right
                    p0Pos + new Vector3(0, bottomY.x, -size.z) ,       // Top-left
                    p0Pos + new Vector3(size.x, bottomY.y, -size.z)    // Top-right
                };
                bool mirrorTriangle =
                    yBottomValue.x > yBottomValue.y ||
                    yBottomValue.w > yBottomValue.z;
                addTriangles = GetTriangle(vertices.Count, mirrorTriangle);
                addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
            }
            else
            {
                return;
            }

            var addUVs2 = new Vector2[]
            {
                tiling,
                tiling,
                tiling,
                tiling
            };
            var rotation = new Vector2(rotationValue, 0);
            var addUVs3 = new Vector2[]
            {
                rotation,
                rotation,
                rotation,
                rotation
            };

            vertices.AddRange(addVertices);
            triangles.AddRange(addTriangles);
            uvs.AddRange(addUVs);
            uvs2.AddRange(addUVs2);
            uvs3.AddRange(addUVs3);
        }

        private int[] GetTriangle(int startIndex, bool isMirror)
        {
            if (!isMirror)
            {
                return new int[]
                {
                    startIndex + 0, startIndex + 2, startIndex + 1,
                    startIndex + 2, startIndex + 3, startIndex + 1
                };
            }
            else
            {
                return new int[]
                {
                    startIndex + 0, startIndex + 2, startIndex + 3,
                    startIndex + 0, startIndex + 3, startIndex + 1
                };
            }
        }

        #endregion

        private Mesh _previewTextureMesh = null;
        public Texture GetBlockPreviewTexture(
            BlockTemplateRuntimeData blockTemplate,
            Material material,
            Vector2 textureSize,
            Vector3 blockSize,
            Vector3 cameraPos,
            Quaternion cameraRotation)
        {
            var worldBlockPivotPos = Vector3.zero - blockSize / 2;

            Vector4 yTopValue = Vector4.one;
            Vector4 yBottomValue = Vector4.zero;

            if (_previewTextureMesh == null)
            {
                _previewTextureMesh = new Mesh();
            }

            var mesh = _previewTextureMesh;
            mesh.Clear();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var uvs2 = new List<Vector2>();
            var uvs3 = new List<Vector2>();

            // +x
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                blockSize,
                Vector3Int.right,
                yTopValue,
                yBottomValue,
                blockTemplate.PXTiling,
                0);

            // -x
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                blockSize,
                Vector3Int.left,
                yTopValue,
                yBottomValue,
                blockTemplate.NXTiling,
                0);

            // +z
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                blockSize,
                Vector3Int.forward,
                yTopValue,
                yBottomValue,
                blockTemplate.PZTiling,
                0);

            // -z
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                blockSize,
                Vector3Int.back,
                yTopValue,
                yBottomValue,
                blockTemplate.NZTiling,
                0);

            // +y
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                blockSize,
                Vector3Int.up,
                yTopValue,
                yBottomValue,
                blockTemplate.PYTiling,
                0);

            // -y
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                blockSize,
                Vector3Int.down,
                yTopValue,
                yBottomValue,
                blockTemplate.NYTiling,
                0);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.uv2 = uvs2.ToArray();
            mesh.uv3 = uvs3.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            return EditorPreviewUtility.GenPreviewTexture(
                mesh,
                material,
                textureSize,
                cameraPos,
                cameraRotation,
                Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));
        }
    }
}
