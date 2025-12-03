using AssetModule;
using Extension;
using GameSystem;
using Logging;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace TerrainModule
{
    public class ChunkController
    {
        private ChunkData _data;

        private Transform _parent;
        private GameObject _go;
        private Transform _transform;
        private Mesh _chunkMesh;
        private MeshFilter _filter;
        private MeshRenderer _renderer;
        private MeshCollider _collider;

        private TerrainEnvironmentController _environmentController;

        public ChunkController(Transform parent, string terrainName, ChunkData data, Material material, TerrainEnvironmentController environmentController)
        {
            _parent = parent;
            _data = data;
            _environmentController = environmentController;
            var mesh = AssetSystem.LoadAsset(data.Mesh);
            if (mesh == null)
            {
                Log.LogError($"ChunkController.construct error mesh not found Name:{terrainName} MeshPath:{data.Mesh.ResourcePath}");
            }
            else
            {
                _chunkMesh = mesh;
                _go = new GameObject(mesh.name);
                _transform = _go.transform;
                _filter = _go.AddComponent<MeshFilter>();
                _renderer = _go.AddComponent<MeshRenderer>();
                _collider = _go.AddComponent<MeshCollider>();

                _transform.SetParent(parent);
                _transform.Reset();
                _filter.sharedMesh = _chunkMesh;
                _renderer.sharedMaterial = material;
                _collider.sharedMesh = _chunkMesh;

                for (int i = 0; i < data.EnvironmentDataList.Count; i++)
                {
                    var envData = data.EnvironmentDataList[i];
                    for (int j = 0; j < envData.InstanceList.Count; j++)
                    {
                        var instanceData = envData.InstanceList[j];
                        if (envData.IsInstanceMesh)
                        {
                            _environmentController.AddMeshInstance(
                                envData.Id,
                                instanceData.InstanceId,
                                instanceData.Position,
                                instanceData.Rotation,
                                instanceData.Scale);
                        }
                        else
                        {
                            _environmentController.AddPrefabInstance(
                                envData.Id,
                                instanceData.InstanceId,
                                instanceData.Position,
                                instanceData.Rotation,
                                instanceData.Scale);
                        }
                    }
                }
            }
        }
    }

    public class TerrainController : IUpdateTarget
    {
        private Transform _parent;
        private TerrainData _terrainData;
        private Material _terrainMaterial;

        private TerrainEnvironmentController _environmentController = new TerrainEnvironmentController();
        private List<TerrainEnvironmentController.MeshSingleInfo> _meshSingleInfoCache = new List<TerrainEnvironmentController.MeshSingleInfo>();

        private Dictionary<int, ChunkController> _idToChunkController = new Dictionary<int, ChunkController>();

        private Transform _chunkRoot = null;

        public TerrainController(Transform parent, TerrainData data)
        {
            _parent = parent;
            _terrainData = data;
            var chunkRootGo = new GameObject(data.name);
            _environmentController.SetGetAction(GetEnvironmentPrefabInfo, GetEnvironmentMeshInfo);
            _chunkRoot = chunkRootGo.transform;
            _chunkRoot.SetParent(parent);
            _chunkRoot.Reset();
            var terrainMaterial = AssetSystem.LoadAsset<Material>(TerrainDefine.GetResourcesMaterialPath(data.name));
            if (terrainMaterial == null)
            {
                Log.LogError($"TerrainController.construct error TerrainMaterial not found Name:{terrainMaterial}");
            }
            else
            {
                _terrainMaterial = terrainMaterial;
            }
        }

        public void OnEnter()
        {
            LoadAllChunk();
        }

        public void OnExit()
        {

        }

        private void LoadAllChunk()
        {
            for (int i = 0; i < _terrainData.ChunkDataList.Count; i++)
            {
                var chunkData = _terrainData.ChunkDataList[i];
                var chunkController = new ChunkController(_chunkRoot, _terrainData.name, chunkData, _terrainMaterial, _environmentController);
                _idToChunkController.Add(chunkData.Id, chunkController);
            }
        }

        private TerrainEnvironmentController.PrefabInfo GetEnvironmentPrefabInfo(int id)
        {
            var data = _terrainData.GetEnvPrefabData(id);
            if (data == null)
                return default;

            var prefab = AssetSystem.LoadAsset(data.Prefab);
            if (prefab == null)
                return default;
            return new TerrainEnvironmentController.PrefabInfo()
            {
                Prefab = prefab,
                Parent = _chunkRoot
            };
        }

        private TerrainEnvironmentController.MeshInfo GetEnvironmentMeshInfo(int id)
        {
            var data = _terrainData.GetEnvInsMeshData(id);
            if (data == null)
                return default;

            _meshSingleInfoCache.Clear();
            for (int i = 0; i < data.MeshSingleDataList.Count; i++)
            {
                var meshData = data.MeshSingleDataList[i];
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

        #region IUpdateTarget

        public void DoUpdate()
        {
            _environmentController.Update();
        }

        public void DoFixedUpdate()
        {

        }

        public void DoLateUpdate()
        {

        }

        public void DoOnGUI()
        {

        }

        public void DoDrawGizmos()
        {

        }

        #endregion
    }

    public class TerrainManager : IUpdateTarget
    {
        private Dictionary<string, TerrainController> _nameToTerrainController = new Dictionary<string, TerrainController>();
        private TerrainController _curTerrainController = null;

        private Transform _terrainRoot;

        public void Init(Transform parent)
        {
            var terrainRootGo = new GameObject("TerrainManager");
            _terrainRoot = terrainRootGo.transform;
            _terrainRoot.SetParent(parent);
            _terrainRoot.Reset();
        }

        public void Enter(string terrainName)
        {
            if (!_nameToTerrainController.TryGetValue(terrainName, out var controller))
            {
                var assetData = AssetSystem.LoadAsset<TerrainData>(TerrainDefine.GetResourcesTerrainDataPath(terrainName));
                if (assetData == null)
                {
                    Log.LogError($"TerrainManager.Enter Error, TerrainData not found Name:{terrainName}");
                    return;
                }
                assetData.name = terrainName;
                controller = new TerrainController(_terrainRoot, assetData);
                _nameToTerrainController.Add(terrainName, controller);
            }

            if (_curTerrainController != null)
                _curTerrainController.OnExit();

            _curTerrainController = controller;
            _curTerrainController.OnEnter();
        }

        #region IUpdateTarget

        void IUpdateTarget.DoUpdate()
        {
            if (_curTerrainController == null)
                return;

            _curTerrainController.DoUpdate();
        }

        void IUpdateTarget.DoFixedUpdate()
        {
            if (_curTerrainController == null)
                return;

            _curTerrainController.DoFixedUpdate();
        }

        void IUpdateTarget.DoLateUpdate()
        {
            if (_curTerrainController == null)
                return;

            _curTerrainController.DoLateUpdate();
        }

        void IUpdateTarget.DoOnGUI()
        {
            if (_curTerrainController == null)
                return;

            _curTerrainController.DoOnGUI();
        }

        void IUpdateTarget.DoDrawGizmos()
        {
            if (_curTerrainController == null)
                return;

            _curTerrainController.DoDrawGizmos();
        }

        #endregion
    }
}
