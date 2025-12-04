using AssetModule;
using Extension;
using GameSystem;
using Logging;
using System.Collections.Generic;
using UnityEngine;

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
        private ColliderController _colliderController;

        public ChunkController(
            Transform parent,
            string terrainName,
            ChunkData data,
            Material material,
            TerrainEnvironmentController environmentController,
            ColliderController colliderController)
        {
            _parent = parent;
            _data = data;
            _environmentController = environmentController;
            _colliderController = colliderController;
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

                            _colliderController.AddColliderInstance(
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

        private ColliderController _colliderController;
        private List<ColliderController.ColliderSingleInfo> _colliderSingleInfoCache = new List<ColliderController.ColliderSingleInfo>();

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
            _colliderController = new ColliderController(_chunkRoot, GetEnvironmentColliderInfo);
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
                var chunkController = new ChunkController(
                    _chunkRoot,
                    _terrainData.name,
                    chunkData,
                    _terrainMaterial,
                    _environmentController,
                    _colliderController);
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
                    Mesh = AssetSystem.LoadAsset(meshData.Mesh),
                    Material = AssetSystem.LoadAsset(meshData.Material),
                    Matrix = meshData.Matrix
                });
            }
            return new TerrainEnvironmentController.MeshInfo()
            {
                MeshSingleInfoList = _meshSingleInfoCache,
            };
        }

        private ColliderController.ColliderInfo GetEnvironmentColliderInfo(int id)
        {
            var data = _terrainData.GetEnvInsMeshData(id);
            if (data == null)
                return default;

            _colliderSingleInfoCache.Clear();
            for (int i = 0; i < data.ColliderDataList.Count; i++)
            {
                var colliderData = data.ColliderDataList[i];
                _colliderSingleInfoCache.Add(new ColliderController.ColliderSingleInfo()
                {
                    ColliderType = colliderData.ColliderType,
                    Center = colliderData.Center,
                    Size = colliderData.Size,
                    Radius = colliderData.Radius,
                    Height = colliderData.Height,
                    Direction = colliderData.Direction,
                    Position = colliderData.Position,
                    Rotation = colliderData.Rotation,
                    Scale = colliderData.Scale,
                });
            }
            return new ColliderController.ColliderInfo()
            {
                ColliderSingleInfoList = _colliderSingleInfoCache,
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
