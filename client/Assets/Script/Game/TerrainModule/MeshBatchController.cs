using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule
{
    public class MeshBatchData
    {
        public List<Mesh> MeshList = new List<Mesh>();
        public List<Material> MaterialList = new List<Material>();
    }

    public class MeshBatchController
    {
        private static List<Matrix4x4> _matrixListCache = new List<Matrix4x4>();
        private static Matrix4x4[] _matrixBuffer = new Matrix4x4[512];

        public int Id;
        private MeshBatchData _mashBatchData = new MeshBatchData();
        private Dictionary<int, Matrix4x4> _idToMatrix = new Dictionary<int, Matrix4x4>();

        private bool _batchDirty = false;

        public void AddMeshBatchData(Mesh mesh, Material material)
        {
            _mashBatchData.MeshList.Add(mesh);
            _mashBatchData.MaterialList.Add(material);
        }

        public void ClearMeshBatchData()
        {
            _mashBatchData.MeshList.Clear();
            _mashBatchData.MaterialList.Clear();
        }

        public void Clear()
        {
            _idToMatrix.Clear();
            _matrixListCache.Clear();
            ClearMeshBatchData();
        }

        public void RegisterMesh(int id, Matrix4x4 matrix)
        {
            if (_idToMatrix.ContainsKey(id))
            {
                _idToMatrix[id] = matrix;
            }
            else
            {
                _idToMatrix.Add(id, matrix);
            }
            _batchDirty = true;
        }

        public void UpdateMesh(int id, Matrix4x4 matrix)
        {
            if (_idToMatrix.ContainsKey(id))
            {
                _idToMatrix[id] = matrix;
                _batchDirty = true;
            }
        }

        public void UnRegisterMesh(int id)
        {
            if (_idToMatrix.Remove(id))
            {
                _batchDirty = true;
            }
        }

        public void UpdateInstance()
        {
            UpdateMatrix();

            var totalCount = _matrixListCache.Count;
            var startIndex = 0;

            while (startIndex < totalCount)
            {
                int batchCount = Mathf.Min(
                    totalCount - startIndex,
                    _matrixBuffer.Length
                );

                _matrixListCache.CopyTo(
                    startIndex,
                    _matrixBuffer,
                    0,
                    batchCount
                );

                for (int i = 0; i < _mashBatchData.MeshList.Count; i++)
                {
                    var mesh = _mashBatchData.MeshList[i];
                    var material = _mashBatchData.MaterialList[i];
                    Graphics.DrawMeshInstanced(
                    mesh,
                    0,
                    material,
                    _matrixBuffer,
                    batchCount);
                }

                startIndex += batchCount;
            }
        }

        private void UpdateMatrix()
        {
            if (!_batchDirty)
                return;

            _matrixListCache.Clear();
            foreach (var matrix in _idToMatrix.Values)
            {
                _matrixListCache.Add(matrix);
            }

            _batchDirty = false;
        }
    }
}
