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

        public ChunkController(Transform parent, string terrainName, ChunkData data, Material material)
        {
            _parent = parent;
            _data = data;
            var mesh = AssetSystem.LoadAsset<Mesh>(TerrainDefine.GetResourcesChunkMeshPath(terrainName, data.MeshName));
            if (mesh == null)
            {
                Log.LogError($"ChunkController.construct error mesh not found Name:{terrainName} MeshName:{data.MeshName}");
            }
            else
            {
                _chunkMesh = mesh;
                _go = new GameObject(data.MeshName);
                _transform = _go.transform;
                _filter = _go.AddComponent<MeshFilter>();
                _renderer = _go.AddComponent<MeshRenderer>();
                _collider = _go.AddComponent<MeshCollider>();

                _transform.SetParent(parent);
                _transform.Reset();
                _filter.sharedMesh = _chunkMesh;
                _renderer.sharedMaterial = material;
                _collider.sharedMesh = _chunkMesh;
            }
        }
    }

    public class TerrainController : IUpdateTarget
    {
        private TerrainData _terrainData;
        private Material _terrainMaterial;

        private Dictionary<int, ChunkController> _idToChunkController = new Dictionary<int, ChunkController>();

        private Transform _chunkRoot = null;

        public TerrainController(Transform parent, TerrainData data)
        {
            _terrainData = data;
            var chunkRootGo = new GameObject(data.name);
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
                _idToChunkController.Add(chunkData.Id, new ChunkController(_chunkRoot, _terrainData.name, chunkData, _terrainMaterial));
            }
        }

        #region IUpdateTarget

        public void DoUpdate()
        {

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
