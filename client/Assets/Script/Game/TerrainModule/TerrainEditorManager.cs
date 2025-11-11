using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class TerrainEditorManager : MonoBehaviour
    {
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
                    DestroyImmediate(Obj);
                MeshFilter = null;
                Mesh = null;
            }
        }

        private TerrainEditRuntimeData _curEditData;
        private Dictionary<int, ChunkPreviewMesh> _chunkIdToPreviewMeshDic = new Dictionary<int, ChunkPreviewMesh>();

        public void Init(TerrainEditRuntimeData editData)
        {
            _curEditData = editData;
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            //重建
            RebuildAllPreviewMesh();
        }

        private void RebuildAllPreviewMesh()
        {
            if (_curEditData == null)
                return;

            //Clear Origin
            foreach (var previewMesh in _chunkIdToPreviewMeshDic.Values)
            {
                previewMesh.Clear();
            }
            _chunkIdToPreviewMeshDic.Clear();

            foreach (var chunkId in _curEditData.IdToChunkEditData.Keys)
            {
                RefreshChunkMesh(chunkId);
            }
        }

        private void CreateBlock(
            List<Vector3> vertices,
            List<int> triangles,
            List<Vector3> normals,
            List<Vector2> uvs,
            ChunkEditRuntimeData chunkEditRuntimeData,
            BlockEditRuntimeData blockEditRuntimeData)
        {
            var chunkId = chunkEditRuntimeData.Id;
            var blockId = blockEditRuntimeData.Id;
            var size = _curEditData.BlockSize;
            var worldBlockCoord = _curEditData.GetWorldBlockCoordWithId(chunkId, blockId);
            var worldBlockPivotPos = worldBlockCoord * size;

            BlockEditRuntimeData refBlockData = null;
            // +x
            var worldRefBlockCoord = worldBlockCoord + Vector3Int.right;
            if (!_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                var p0Pos = worldBlockPivotPos + new Vector3(size.x , 0, 0);
                var addVertices = new Vector3[]
                {
                    p0Pos,                                   // Bottom-left
                    p0Pos + new Vector3(0, 0, size.z),       // Bottom-right
                    p0Pos + new Vector3(0, size.y, 0),       // Top-left
                    p0Pos + new Vector3(0, size.y, size.z)   // Top-right
                };
                var curVerticesCount = vertices.Count;
                var  addTriangles = new int[]
                {
                    curVerticesCount + 0, curVerticesCount + 2, curVerticesCount + 1,
                    curVerticesCount + 2, curVerticesCount + 3, curVerticesCount + 1
                };
                var addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
                vertices.AddRange(addVertices);
                triangles.AddRange(addTriangles);
                uvs.AddRange(addUVs);
            }

            // -x
            worldRefBlockCoord = worldBlockCoord + Vector3Int.left;
            if (!_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                var p0Pos = worldBlockPivotPos + new Vector3(0, 0, size.z);
                var addVertices = new Vector3[]
                {
                    p0Pos,                                    // Bottom-left
                    p0Pos + new Vector3(0, 0, -size.z),       // Bottom-right
                    p0Pos + new Vector3(0, size.y, 0),        // Top-left
                    p0Pos + new Vector3(0, size.y, -size.z)   // Top-right
                };
                var curVerticesCount = vertices.Count;
                var addTriangles = new int[]
                {
                    curVerticesCount + 0, curVerticesCount + 2, curVerticesCount + 1,
                    curVerticesCount + 2, curVerticesCount + 3, curVerticesCount + 1
                };
                var addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
                vertices.AddRange(addVertices);
                triangles.AddRange(addTriangles);
                uvs.AddRange(addUVs);
            }

            // +z
            worldRefBlockCoord = worldBlockCoord + Vector3Int.forward;
            if (!_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                var p0Pos = worldBlockPivotPos + new Vector3(size.x, 0, size.z);
                var addVertices = new Vector3[]
                {
                    p0Pos,                                    // Bottom-left
                    p0Pos + new Vector3(-size.x, 0, 0),       // Bottom-right
                    p0Pos + new Vector3(0, size.y, 0) ,       // Top-left
                    p0Pos + new Vector3(-size.x, size.y, 0)   // Top-right
                };
                var curVerticesCount = vertices.Count;
                var addTriangles = new int[]
                {
                    curVerticesCount + 0, curVerticesCount + 2, curVerticesCount + 1,
                    curVerticesCount + 2, curVerticesCount + 3, curVerticesCount + 1
                };
                var addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
                vertices.AddRange(addVertices);
                triangles.AddRange(addTriangles);
                uvs.AddRange(addUVs);
            }

            // -z
            worldRefBlockCoord = worldBlockCoord + Vector3Int.back;
            if (!_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                var p0Pos = worldBlockPivotPos;
                var addVertices = new Vector3[]
                {
                    p0Pos,                                    // Bottom-left
                    p0Pos + new Vector3(size.x, 0, 0),        // Bottom-right
                    p0Pos + new Vector3(0, size.y, 0) ,       // Top-left
                    p0Pos + new Vector3(size.x, size.y, 0)    // Top-right
                };
                var curVerticesCount = vertices.Count;
                var addTriangles = new int[]
                {
                    curVerticesCount + 0, curVerticesCount + 2, curVerticesCount + 1,
                    curVerticesCount + 2, curVerticesCount + 3, curVerticesCount + 1
                };
                var addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
                vertices.AddRange(addVertices);
                triangles.AddRange(addTriangles);
                uvs.AddRange(addUVs);
            }

            // +y
            worldRefBlockCoord = worldBlockCoord + Vector3Int.up;
            if (!_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                var p0Pos = worldBlockPivotPos + new Vector3(0, size.y, 0);
                var addVertices = new Vector3[]
                {
                    p0Pos,                                    // Bottom-left
                    p0Pos + new Vector3(size.x, 0, 0),        // Bottom-right
                    p0Pos + new Vector3(0, 0, size.z) ,       // Top-left
                    p0Pos + new Vector3(size.x, 0, size.z)    // Top-right
                };
                var curVerticesCount = vertices.Count;
                var addTriangles = new int[]
                {
                    curVerticesCount + 0, curVerticesCount + 2, curVerticesCount + 1,
                    curVerticesCount + 2, curVerticesCount + 3, curVerticesCount + 1
                };
                var addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
                vertices.AddRange(addVertices);
                triangles.AddRange(addTriangles);
                uvs.AddRange(addUVs);
            }

            // -y
            worldRefBlockCoord = worldBlockCoord + Vector3Int.down;
            if (!_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                var p0Pos = worldBlockPivotPos + new Vector3(0, 0, size.z);
                var addVertices = new Vector3[]
                {
                    p0Pos,                                     // Bottom-left
                    p0Pos + new Vector3(size.x, 0, 0),         // Bottom-right
                    p0Pos + new Vector3(0, 0, -size.z) ,       // Top-left
                    p0Pos + new Vector3(size.x, 0, -size.z)    // Top-right
                };
                var curVerticesCount = vertices.Count;
                var addTriangles = new int[]
                {
                    curVerticesCount + 0, curVerticesCount + 2, curVerticesCount + 1,
                    curVerticesCount + 2, curVerticesCount + 3, curVerticesCount + 1
                };
                var addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
                vertices.AddRange(addVertices);
                triangles.AddRange(addTriangles);
                uvs.AddRange(addUVs);
            }
        }

        public void RefreshChunkMesh(int chunkId)
        {
            if (_curEditData == null)
                return;

            if (!_chunkIdToPreviewMeshDic.TryGetValue(chunkId, out var previewMesh))
            {
                previewMesh = new ChunkPreviewMesh(chunkId, transform);
                _chunkIdToPreviewMeshDic.Add(chunkId, previewMesh);
            }
            previewMesh.Mesh.Clear();

            if (!_curEditData.IdToChunkEditData.TryGetValue(chunkId, out var chunkEditData))
                return;

            var mesh = previewMesh.Mesh;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var chunkPivotPos = _curEditData.GetChunkPivotPositionWithId(chunkId);
            foreach (var blockEditDataPair in chunkEditData.IdToBlockEditData)
            {
                var blockEditData = blockEditDataPair.Value;
                CreateBlock(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    chunkEditData,
                    blockEditData);


                //var inChunkBlockPos = _curEditData.GetBlockInChunkPivotPositionWithId(blockEditData.Id);
                //var worldPos = chunkPivotPos + inChunkBlockPos;
                //CreateBlock(
                //    vertices,
                //    triangles,
                //    normals,
                //    uvs,
                //    worldPos,
                //    _curEditData.BlockSize);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            previewMesh.MeshRenderer.sharedMaterial = _curEditData.TerrainMaterial;
        }
    }
}
