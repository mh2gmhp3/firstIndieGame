using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule
{
    public class TerrainEnvironmentController
    {
        public struct PrefabInfo
        {
            public GameObject Prefab;
            public Transform Parent;

            public bool IsValid()
            {
                if (Prefab == null)
                    return false;
                return true;
            }
        }

        public struct MeshSingleInfo
        {
            public Mesh Mesh;
            public Material Material;
            public Matrix4x4 Matrix;

            public bool IsValid()
            {
                if (Mesh == null)
                    return false;
                if (Material == null)
                    return false;
                return true;
            }
        }

        public struct MeshInfo
        {
            public List<MeshSingleInfo> MeshSingleInfoList;

            public bool IsValid()
            {
                if (MeshSingleInfoList == null)
                    return false;
                for (int i = 0; i < MeshSingleInfoList.Count; i++)
                {
                    if (!MeshSingleInfoList[i].IsValid())
                        return false;
                }
                return true;
            }
        }

        private Dictionary<int, PrefabController> _idToPrefabController = new Dictionary<int, PrefabController>();
        private Dictionary<int, MeshBatchController> _idToInsMeshBatchController = new Dictionary<int, MeshBatchController>();
        private List<MeshBatchController> _meshBatchControllerList = new List<MeshBatchController>();

        private Func<int, PrefabInfo> _prefabGetAction;
        private Func<int, MeshInfo> _meshGetAction;

        public void SetGetAction(Func<int, PrefabInfo> prefabGetAction, Func<int, MeshInfo> meshGetAction)
        {
            _prefabGetAction = prefabGetAction;
            _meshGetAction = meshGetAction;
        }

        public void AddPrefabInstance(int id, int instanceId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (_prefabGetAction == null)
                return;

            if (!_idToPrefabController.TryGetValue(id, out var prefabController))
            {
                var prefabInfo = _prefabGetAction(id);
                if (!prefabInfo.IsValid())
                    return;
                prefabController = new PrefabController(prefabInfo.Prefab, prefabInfo.Parent);
                _idToPrefabController.Add(id, prefabController);
            }
            prefabController.AddInstance(instanceId, position, rotation, scale);
        }

        public void RemovePrefabInstance(int id, int instanceId)
        {
            if (!_idToPrefabController.TryGetValue(id, out var prefabController))
                return;

            prefabController.RemoveInstance(instanceId);
        }

        public void AddMeshInstance(int id, int instanceId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (_meshGetAction == null)
                return;

            if (!_idToInsMeshBatchController.TryGetValue(id, out var meshBatchController))
            {
                var meshInfo = _meshGetAction(id);
                if (!meshInfo.IsValid())
                    return;
                meshBatchController = new MeshBatchController();
                for (int i = 0; i < meshInfo.MeshSingleInfoList.Count; i++)
                {
                    var meshSingleInfo = meshInfo.MeshSingleInfoList[i];
                    meshBatchController.AddMeshBatchData(meshSingleInfo.Mesh, meshSingleInfo.Material, meshSingleInfo.Matrix);
                    _meshBatchControllerList.Add(meshBatchController);
                }
                _idToInsMeshBatchController.Add(id, meshBatchController);
            }
            meshBatchController.AddInstance(instanceId, Matrix4x4.TRS(position, rotation, scale));
        }

        public void RemoveMeshInstance(int id, int instanceId)
        {
            if (!_idToInsMeshBatchController.TryGetValue(id, out var meshBatchController))
                return;
            meshBatchController.RemoveInstance(instanceId);
        }

        public void Update()
        {
            for (int i = 0; i < _meshBatchControllerList.Count; i++)
            {
                _meshBatchControllerList[i].UpdateInstance();
            }
        }
    }
}
