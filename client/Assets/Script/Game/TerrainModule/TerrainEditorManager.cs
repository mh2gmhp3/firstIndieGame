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
            Vector3 worldPos,
            Vector3 size)
        {
            // +x
            Vector3[] addVertices = new Vector3[]
            {
                worldPos, // Bottom-left
                new Vector3(worldPos.x + size.x, worldPos.y, worldPos.z),  // Bottom-right
                new Vector3(worldPos.x , worldPos.y + size.y, worldPos.z),  // Top-left
                new Vector3(worldPos.x + size.x, worldPos.y + size.y, worldPos.z)   // Top-right
            };
            var curVerticesCount = vertices.Count;
            int[] addTriangles = new int[]
                {
                    curVerticesCount + 0, curVerticesCount + 2, curVerticesCount + 1,
                    curVerticesCount + 2, curVerticesCount + 3, curVerticesCount + 1
                };
            Vector2[] addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
            vertices.AddRange(addVertices);
            triangles.AddRange(addTriangles);
            uvs.AddRange(addUVs);

            // -x
            addVertices = new Vector3[]
            {
                worldPos + new Vector3(size.x, 0, size.z), // Bottom-left
                new Vector3(worldPos.x + size.x, worldPos.y, worldPos.z) + new Vector3(size.x, 0, size.z),  // Bottom-right
                new Vector3(worldPos.x , worldPos.y + size.y, worldPos.z) + new Vector3(size.x, 0, size.z),  // Top-left
                new Vector3(worldPos.x + size.x, worldPos.y + size.y, worldPos.z) + new Vector3(size.x, 0, size.z)   // Top-right
            };
            curVerticesCount = vertices.Count;
            addTriangles = new int[]
                {
                    curVerticesCount + 0, curVerticesCount + 2, curVerticesCount + 1,
                    curVerticesCount + 2, curVerticesCount + 3, curVerticesCount + 1
                };
            addUVs = new Vector2[]
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
                var inChunkBlockPos = _curEditData.GetBlockInChunkPivotPositionWithId(blockEditData.Id);
                var worldPos = chunkPivotPos + inChunkBlockPos;
                CreateBlock(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    worldPos,
                    _curEditData.BlockSize);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
        }
    }
}
