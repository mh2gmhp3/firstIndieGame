using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using UnityEngine.ProBuilder;

namespace TerrainModule.Editor
{
    public class TerrainEditorManager : MonoBehaviour
    {
        private TerrainEditRuntimeData _curEditData;

        private GameObject _previewTerrainObj;
        private MeshFilter _previewTerrainMeshFilter;
        private Mesh _previewTerrainMesh;

        public void Init(TerrainEditRuntimeData editData)
        {
            _curEditData = editData;
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            //重建
            if (_previewTerrainObj != null)
                DestroyImmediate(_previewTerrainObj);
            _previewTerrainObj = new GameObject("PreviewTerrain");
            _previewTerrainObj.transform.SetParent(transform);
            _previewTerrainMeshFilter = _previewTerrainObj.AddComponent<MeshFilter>();
            _previewTerrainObj.AddComponent<MeshRenderer>();
            if (_previewTerrainMesh != null)
                DestroyImmediate(_previewTerrainMesh);
            _previewTerrainMesh = new Mesh();
            _previewTerrainMesh.name = "PreviewMesh";
            _previewTerrainMesh.MarkDynamic();
            _previewTerrainMeshFilter.mesh = _previewTerrainMesh;

            RebuildAllPreviewMesh();
        }

        private void RebuildAllPreviewMesh()
        {
            if (_curEditData == null || _previewTerrainMesh == null)
                return;

            _previewTerrainMesh.Clear();
            //var vertices = new List<Vector3>();
            //var triangles = new List<int>();
            //var normals = new List<Vector3>();
            //var uvs = new List<Vector2>();
            //var blockNum = _curEditData.GetBlockNum();
            //for (int yIndex = 0; yIndex < blockNum.y; yIndex++)
            //{
            //    for (int zIndex = 0; zIndex < blockNum.z; zIndex++)
            //    {
            //        for (int xIndex = 0; xIndex < blockNum.x; xIndex++)
            //        {
            //            CreateBlock(
            //                vertices,
            //                triangles,
            //                normals,
            //                uvs,
            //                new Vector3(
            //                    xIndex * _curEditData.BlockSize.x,
            //                    yIndex * _curEditData.BlockSize.y,
            //                    zIndex * _curEditData.BlockSize.z),
            //                _curEditData.BlockSize);
            //        }
            //    }
            //}

            //_previewTerrainMesh.vertices = vertices.ToArray();
            //_previewTerrainMesh.triangles = triangles.ToArray();
            //_previewTerrainMesh.uv = uvs.ToArray();

            //_previewTerrainMesh.RecalculateNormals();
            //_previewTerrainMesh.RecalculateTangents();
            //_previewTerrainMesh.RecalculateBounds();
        }

        private void CreateBlock(
            List<Vector3> vertices,
            List<int> triangles,
            List<Vector3> normals,
            List<Vector2> uvs,
            Vector3 worldPos,
            Vector3 size)
        {
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
        }

        private void OnDrawGizmos()
        {
            if (_curEditData == null)
                return;

            var chunkSize = _curEditData.GetChunkSize();
            var centerRedress = new Vector3(
                chunkSize.x / 2f,
                chunkSize.y / 2,
                chunkSize.z / 2);
            for (int yIndex = 0; yIndex < _curEditData.ChunkNum.y; yIndex++)
            {
                for (int zIndex = 0; zIndex < _curEditData.ChunkNum.z; zIndex++)
                {
                    for (int xIndex = 0; xIndex < _curEditData.ChunkNum.x; xIndex++)
                    {
                        var center = new Vector3(
                                xIndex * chunkSize.x,
                                yIndex * chunkSize.y,
                                zIndex * chunkSize.z) + centerRedress;
                        Gizmos.DrawWireCube(center, chunkSize);
                    }
                }
            }
        }
    }
}
