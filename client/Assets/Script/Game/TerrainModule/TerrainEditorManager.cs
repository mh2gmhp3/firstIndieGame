using Extension;
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

        public void RebuildAllPreviewMesh()
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
            List<Vector2> uvs2,
            List<Vector2> uvs3,
            ChunkEditRuntimeData chunkEditRuntimeData,
            BlockEditRuntimeData blockEditRuntimeData)
        {
            var chunkId = chunkEditRuntimeData.Id;
            var blockId = blockEditRuntimeData.Id;
            var size = _curEditData.BlockSize;
            var worldBlockCoord = _curEditData.GetWorldBlockCoordWithId(chunkId, blockId);
            var worldBlockPivotPos = worldBlockCoord * size;

            BlockEditRuntimeData refBlockData = null;

            bool createFace = false;
            Vector4 topYValue = blockEditRuntimeData.YTopValue;
            Vector4 bottomTValue = blockEditRuntimeData.YBottomValue;
            Vector4 topY = size.y * topYValue;
            Vector4 bottomY = size.y * bottomTValue;

            var tiling = new Vector2(0, 0);
            var rotation = new Vector2(0, 0);

            //方向沒方塊必須建立面
            //有方塊必須完全共面才不需建立 不考慮交錯要補面問題
            // +x
            var worldRefBlockCoord = worldBlockCoord + Vector3Int.right;
            if (_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
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
                var p0Pos = worldBlockPivotPos + new Vector3(size.x , 0, 0);
                var addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.y, 0),                                   // Bottom-left
                    p0Pos + new Vector3(0, bottomY.w, size.z),                              // Bottom-right
                    p0Pos + new Vector3(0, topY.y, 0),                                      // Top-left
                    p0Pos + new Vector3(0, topY.w, size.z)                                  // Top-right
                };
                var addTriangles = GetTriangle(vertices.Count, false);
                var addUVs = new Vector2[]
                {
                    new Vector2(0, bottomTValue.y),
                    new Vector2(1 , bottomTValue.w),
                    new Vector2(0, topYValue.y),
                    new Vector2(1, topYValue.w)
                };
                var addUVs2 = new Vector2[]
                {
                    tiling,
                    tiling,
                    tiling,
                    tiling
                };
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

            // -x
            worldRefBlockCoord = worldBlockCoord + Vector3Int.left;
            if (_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
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
                var p0Pos = worldBlockPivotPos + new Vector3(0, 0, size.z);
                var addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.z, 0),                                    // Bottom-left
                    p0Pos + new Vector3(0, bottomY.x, -size.z),                              // Bottom-right
                    p0Pos + new Vector3(0, topY.z, 0),                                       // Top-left
                    p0Pos + new Vector3(0, topY.x, -size.z)                                  // Top-right
                };
                var addTriangles = GetTriangle(vertices.Count, false);
                var addUVs = new Vector2[]
                {
                    new Vector2(0, bottomTValue.z),
                    new Vector2(1 , bottomTValue.x),
                    new Vector2(0, topYValue.z),
                    new Vector2(1, topYValue.x)
                };
                var addUVs2 = new Vector2[]
                {
                    tiling,
                    tiling,
                    tiling,
                    tiling
                };
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

            // +z
            worldRefBlockCoord = worldBlockCoord + Vector3Int.forward;
            if (_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
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
                var p0Pos = worldBlockPivotPos + new Vector3(size.x, 0, size.z);
                var addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.w, 0),                                    // Bottom-left
                    p0Pos + new Vector3(-size.x, bottomY.z, 0),                              // Bottom-right
                    p0Pos + new Vector3(0, topY.w, 0) ,                                      // Top-left
                    p0Pos + new Vector3(-size.x, topY.z, 0)                                  // Top-right
                };
                var addTriangles = GetTriangle(vertices.Count, false);
                var addUVs = new Vector2[]
                {
                    new Vector2(0, bottomTValue.w),
                    new Vector2(1 , bottomTValue.z),
                    new Vector2(0, topYValue.w),
                    new Vector2(1, topYValue.z)
                };
                var addUVs2 = new Vector2[]
                {
                    tiling,
                    tiling,
                    tiling,
                    tiling
                };
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

            // -z
            worldRefBlockCoord = worldBlockCoord + Vector3Int.back;
            if (_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
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
                var p0Pos = worldBlockPivotPos;
                var addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.x, 0),                                    // Bottom-left
                    p0Pos + new Vector3(size.x, bottomY.y, 0),                               // Bottom-right
                    p0Pos + new Vector3(0, topY.x, 0) ,                                      // Top-left
                    p0Pos + new Vector3(size.x, topY.y, 0)                                   // Top-right
                };
                var addTriangles = GetTriangle(vertices.Count, false);
                var addUVs = new Vector2[]
                {
                    new Vector2(0, bottomTValue.x),
                    new Vector2(1 , bottomTValue.y),
                    new Vector2(0, topYValue.x),
                    new Vector2(1, topYValue.y)
                };
                var addUVs2 = new Vector2[]
                {
                    tiling,
                    tiling,
                    tiling,
                    tiling
                };
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

            // +y
            worldRefBlockCoord = worldBlockCoord + Vector3Int.up;
            if (_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = blockEditRuntimeData.YTopValue != Vector4.one || refBlockData.YBottomValue != Vector4.zero;
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                var p0Pos = worldBlockPivotPos;
                var addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, topY.x, 0),                                    // Bottom-left
                    p0Pos + new Vector3(size.x, topY.y, 0),                               // Bottom-right
                    p0Pos + new Vector3(0, topY.z, size.z) ,                              // Top-left
                    p0Pos + new Vector3(size.x, topY.w, size.z)                           // Top-right
                };
                bool mirrorTriangle =
                    blockEditRuntimeData.YTopValue.x > blockEditRuntimeData.YTopValue.y ||
                    blockEditRuntimeData.YTopValue.w > blockEditRuntimeData.YTopValue.z;
                var addTriangles = GetTriangle(vertices.Count, mirrorTriangle);
                var addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
                var addUVs2 = new Vector2[]
                {
                    tiling,
                    tiling,
                    tiling,
                    tiling
                };
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

            // -y
            worldRefBlockCoord = worldBlockCoord + Vector3Int.down;
            if (_curEditData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = blockEditRuntimeData.YBottomValue != Vector4.zero || refBlockData.YTopValue != Vector4.one;
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                var p0Pos = worldBlockPivotPos + new Vector3(0, 0, size.z);
                var addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.z, 0),                                     // Bottom-left
                    p0Pos + new Vector3(size.x, bottomY.w, 0),         // Bottom-right
                    p0Pos + new Vector3(0, bottomY.x, -size.z) ,       // Top-left
                    p0Pos + new Vector3(size.x, bottomY.y, -size.z)    // Top-right
                };
                bool mirrorTriangle =
                    blockEditRuntimeData.YBottomValue.x > blockEditRuntimeData.YBottomValue.y ||
                    blockEditRuntimeData.YBottomValue.w > blockEditRuntimeData.YBottomValue.z;
                var addTriangles = GetTriangle(vertices.Count, mirrorTriangle);
                var addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
                var addUVs2 = new Vector2[]
                {
                    tiling,
                    tiling,
                    tiling,
                    tiling
                };
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
            var uvs2 = new List<Vector2>();
            var uvs3 = new List<Vector2>();
            var chunkPivotPos = _curEditData.GetChunkPivotPositionWithId(chunkId);
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

            previewMesh.MeshRenderer.sharedMaterial = _curEditData.TerrainMaterial;
        }
    }
}
