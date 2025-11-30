using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule
{
    public class MeshBatch
    {
        public Mesh Mesh;
        public Material Material;
        public Matrix4x4 Matrix;

        public List<Matrix4x4> InsMatrices = new List<Matrix4x4>();

        public MeshBatch(Mesh mesh, Material material, Matrix4x4 matrix)
        {
            Mesh = mesh;
            Material = material;
            Matrix = matrix;
        }
    }

    public class MeshBatchController
    {
        private static Matrix4x4[] _matrixBuffer = new Matrix4x4[512];

        public int Id;
        private Dictionary<int, Matrix4x4> _idToMatrix = new Dictionary<int, Matrix4x4>();
        private List<MeshBatch> _meshBatch = new List<MeshBatch>();

        private bool _batchDirty = false;

        public void AddMeshBatchData(Mesh mesh, Material material, Matrix4x4 matrix)
        {
            _meshBatch.Add(new MeshBatch(mesh, material, matrix));
        }

        public void ClearMeshBatchData()
        {
            _meshBatch.Clear();
        }

        public void Clear()
        {
            _idToMatrix.Clear();
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

            var totalCount = _idToMatrix.Count;
            var startIndex = 0;

            while (startIndex < totalCount)
            {
                int batchCount = Mathf.Min(
                    totalCount - startIndex,
                    _matrixBuffer.Length);

                for (int i = 0; i < _meshBatch.Count; i++)
                {
                    var meshBatch = _meshBatch[i];
                    meshBatch.InsMatrices.CopyTo(
                        startIndex,
                        _matrixBuffer,
                        0,
                        batchCount);

                    Graphics.DrawMeshInstanced(
                    meshBatch.Mesh,
                    0,
                    meshBatch.Material,
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

            for (int i = 0; i < _meshBatch.Count; i++)
            {
                var meshBatch = _meshBatch[i];
                meshBatch.InsMatrices.Clear();
                foreach (var matrix in _idToMatrix.Values)
                {
                    meshBatch.InsMatrices.Add(matrix * meshBatch.Matrix);
                }
            }

            _batchDirty = false;
        }
    }
}
