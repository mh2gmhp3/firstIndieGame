using Extension;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utility;
using static TerrainModule.TerrainDefine;

namespace TerrainModule.Editor
{
    public class TerrainEnvironmentIdMapping
    {
        public struct MappingInfo
        {
            public bool IsInsMesh;
            public string CategoryName;
            public string Name;

            public MappingInfo(bool isInsMesh, string categoryName, string name)
            {
                IsInsMesh = isInsMesh;
                CategoryName = categoryName;
                Name = name;
            }
        }

        private Dictionary<string, Dictionary<string, int>> _prefabToId = new Dictionary<string, Dictionary<string, int>>();
        private Dictionary<string, Dictionary<string, int>> _insMeshToId = new Dictionary<string, Dictionary<string, int>>();
        private Dictionary<int, MappingInfo> _idToMappingInfo = new Dictionary<int, MappingInfo>();

        private int _id;

        public void AddMapping(bool isInsMesh, string categoryName, string name)
        {
            var categoryMapping = GetMapping(isInsMesh);
            if (!categoryMapping.TryGetValue(categoryName, out var nameMapping))
            {
                nameMapping = new Dictionary<string, int>();
                categoryMapping.Add(categoryName, nameMapping);
            }
            nameMapping[name] = ++_id;
            _idToMappingInfo[_id] = new MappingInfo(isInsMesh, categoryName, name);
        }

        public bool TryGetId(bool isInsMesh, string categoryName, string name, out int id)
        {
            id = -1;
            var categoryMapping = GetMapping(isInsMesh);
            if (!categoryMapping.TryGetValue(categoryName, out var nameMapping))
                return false;

            return nameMapping.TryGetValue(name, out id);
        }

        public bool TryGetInfo(int id, out MappingInfo info)
        {
            return _idToMappingInfo.TryGetValue(id, out info);
        }

        private Dictionary<string, Dictionary<string, int>> GetMapping(bool isInsMesh)
        {
            return isInsMesh ? _insMeshToId : _prefabToId;
        }

        public void Clear()
        {
            _insMeshToId.Clear();
            _prefabToId.Clear();
            _idToMappingInfo.Clear();
        }
    }

    public enum GetBlockReason
    {
        Success,
        PositionOutOfRange,
        IdOutOfRange,
        ChunkNotCreated,
        BlockNotCreated,
    }

    public enum GetChunkReason
    {
        Success,
        PositionOutOfRange,
        ChunkNotCreated,
    }

    public struct RaycastBlockResult
    {
        public int ChunkId;
        public int BlockId;

        public Vector3Int WorldBlockCoordinates;
        /// <summary>
        /// 被射線檢測到的面方向
        /// </summary>
        public Vector3Int HitFaceNormal;
        public Vector3 HitWorldPosition;
        public bool HaveData;
    }

    public enum RaycastBlockFilterType
    {
        Ok,
        Continue,
        Break
    }

    public class AreaEditRuntimeData
    {
        public int Id;
        public AreaType AreaType;

        public Vector3 WorldPoint;
        public float Radius;

        public AreaEditRuntimeData(int id)
        {
            Id = id;
        }

        public AreaEditRuntimeData(AreaEditData editData)
        {
            Id = editData.Id;
            AreaType = editData.AreaType;

            WorldPoint = editData.WorldPoint;
            Radius = editData.Radius;
        }
    }

    public class AreaGroupEditRuntimeData
    {
        public int Id;
        public string Description;
        public List<AreaEditRuntimeData> AreaList = new List<AreaEditRuntimeData>();

        public AreaGroupEditRuntimeData(int id)
        {
            Id = id;
        }

        public AreaGroupEditRuntimeData(AreaGroupEditData editData)
        {
            Id = editData.Id;
            Description = editData.Description;
            for (int i = 0; i < editData.AreaList.Count; i++)
            {
                AreaList.Add(new AreaEditRuntimeData(editData.AreaList[i]));
            }
        }

        public void AddArea()
        {
            var nextId = GetAreaNextId();
            AreaList.Add(new AreaEditRuntimeData(nextId));
        }

        private int GetAreaNextId()
        {
            var maxId = 0;
            for (int i = 0; i < AreaList.Count; i++)
            {
                var area = AreaList[i];
                if (area.Id > maxId)
                    maxId = area.Id;
            }

            return ++maxId;
        }
    }

    public class EnvironmentInstanceEditRuntimeData
    {
        public int InstanceId;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public EnvironmentInstanceEditRuntimeData(int instanceId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            InstanceId = instanceId;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }

    public class EnvironmentEditRuntimeData
    {
        public bool IsInstanceMesh;
        public string CategoryName;
        public string Name;

        public List<EnvironmentInstanceEditRuntimeData> InstanceList = new List<EnvironmentInstanceEditRuntimeData>();

        public EnvironmentEditRuntimeData(bool isInsMesh, string categoryName, string name)
        {
            IsInstanceMesh = isInsMesh;
            CategoryName = categoryName;
            Name = name;
        }

        public void AddInstance(int instanceId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            InstanceList.Add(new EnvironmentInstanceEditRuntimeData(instanceId, position, rotation, scale));
        }
    }

    public class BlockEditRuntimeData
    {
        //In Chunk
        public int Id;
        public int TemplateId;

        public Vector4 YTopValue = Vector4.one;
        public Vector4 YBottomValue = Vector4.zero;

        public float PXRotation;
        public float NXRotation;

        public float PYRotation;
        public float NYRotation;

        public float PZRotation;
        public float NZRotation;

        public BlockEditRuntimeData(int id, int templateId)
        {
            Id = id;
            TemplateId = templateId;

            YTopValue = Vector4.one;
            YBottomValue = Vector4.zero;
        }

        public BlockEditRuntimeData(int id, int templateId, Vector4 yTopValue, Vector4 yBottomValue)
        {
            Id = id;
            TemplateId = templateId;

            SetYValue(yTopValue, yBottomValue);
        }

        public BlockEditRuntimeData(BlockEditData editData)
        {
            Id = editData.Id;
            TemplateId = editData.TemplateId;

            YTopValue = editData.YTopValue;
            YBottomValue = editData.YBottomValue;
        }

        public void SetYValue(Vector4 topValue, Vector4 bottomValue)
        {
            var clampValue = TerrainEditorUtility.ClampYValue(topValue, bottomValue);
            YTopValue = clampValue.TopValue;
            YBottomValue = clampValue.BottomValue;
        }
    }

    public class ChunkEditRuntimeData
    {
        public int Id;

        public Dictionary<int, BlockEditRuntimeData> IdToBlockEditData = new Dictionary<int, BlockEditRuntimeData>();

        public List<EnvironmentEditRuntimeData> EnvironmentEditDataList = new List<EnvironmentEditRuntimeData>();

        public ChunkEditRuntimeData(int id)
        {
            Id = id;
        }

        public ChunkEditRuntimeData(ChunkEditData editData, ref int evnInstanceId)
        {
            Id = editData.Id;
            IdToBlockEditData.Clear();
            for (int i = 0; i < editData.BlockEditDataList.Count; i++)
            {
                var blockData = editData.BlockEditDataList[i];
                if (blockData == null)
                    continue;
                var blockRuntimeData = new BlockEditRuntimeData(blockData);
                IdToBlockEditData.Add(blockRuntimeData.Id, blockRuntimeData);
            }
            EnvironmentEditDataList.Clear();
            for (int i = 0; i < editData.EnvironmentEditDataList.Count; i++)
            {
                var envData = editData.EnvironmentEditDataList[i];
                var envEditData = GetEnvironmentEditData(envData.IsInstanceMesh, envData.CategoryName, envData.Name);
                for (int j = 0; j < envData.InstanceList.Count; j++)
                {
                    var insData = envData.InstanceList[j];
                    envEditData.AddInstance(++evnInstanceId, insData.Position, insData.Rotation, insData.Scale);
                }
            }
        }

        public void AddEnvironment(int instanceId, bool isInsMesh, string categoryName, string name, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var evnEditData = GetEnvironmentEditData(isInsMesh, categoryName, name);
            evnEditData.AddInstance(instanceId, position, rotation, scale);
        }

        private EnvironmentEditRuntimeData GetEnvironmentEditData(bool isInsMesh, string categoryName, string name)
        {
            for (int i = 0; i < EnvironmentEditDataList.Count; i++)
            {
                var envEditData = EnvironmentEditDataList[i];
                if (envEditData.IsInstanceMesh == isInsMesh &&
                    envEditData.CategoryName == categoryName &&
                    envEditData.Name == name)
                {
                    return envEditData;
                }
            }

            var newEditData = new EnvironmentEditRuntimeData(isInsMesh, categoryName, name);
            EnvironmentEditDataList.Add(newEditData);
            return newEditData;
        }
    }

    /// <summary>
    /// 編輯器運行時的資料
    /// </summary>
    public class TerrainEditRuntimeData
    {
        public string Name;

        public Vector3Int BlockSize = Vector3Int.one;

        public Vector3Int ChunkBlockNum = Vector3Int.one;
        public Vector3Int ChunkNum = Vector3Int.one;

        public BlockTemplateEditData BlockTemplateEditData;
        public EnvironmentTemplateEditData EnvironmentTemplateEditData;

        public Dictionary<int, ChunkEditRuntimeData> IdToChunkEditData = new Dictionary<int, ChunkEditRuntimeData>();

        public List<AreaGroupEditRuntimeData> AreaGroupEditDataList = new List<AreaGroupEditRuntimeData>();

        public BlockTemplateEditRuntimeData BlockTemplateEditRuntimeData { get; private set; }
        public Material TerrainMaterial { get; private set; }

        public EnvironmentTemplateEditRuntimeData EnvironmentTemplateEditRuntimeData { get; private set; }
        public TerrainEnvironmentIdMapping EnvironmentTemplateIdMapping { get; private set; } = new TerrainEnvironmentIdMapping();
        public int EnvironmentInstanceId;

        public TerrainEditRuntimeData(TerrainEditData editData)
        {
            Name = editData.name;
            BlockSize = editData.BlockSize;
            ChunkBlockNum = editData.ChunkBlockNum;
            ChunkNum = editData.ChunkNum;
            BlockTemplateEditData = editData.BlockTemplateEditData;
            EnvironmentTemplateEditData = editData.EnvironmentTemplateEditData;

            IdToChunkEditData.Clear();
            for (int i = 0; i < editData.ChunkEditDataList.Count; i++)
            {
                var chunk = editData.ChunkEditDataList[i];
                if (chunk == null)
                    continue;

                var chunkRuntimeData = new ChunkEditRuntimeData(chunk, ref EnvironmentInstanceId);
                IdToChunkEditData.Add(chunkRuntimeData.Id, chunkRuntimeData);
            }

            AreaGroupEditDataList.Clear();
            for (int i = 0; i < editData.AreaGroupEditDataList.Count; i++)
            {
                AreaGroupEditDataList.Add(new AreaGroupEditRuntimeData(editData.AreaGroupEditDataList[i]));
            }

            RefreshBlockTemplateRuntimeData();
            RefreshEnvironmentTemplateRuntimeData();
        }

        public void RefreshBlockTemplateRuntimeData()
        {
            if (BlockTemplateEditData == null)
                return;
            BlockTemplateEditRuntimeData = new BlockTemplateEditRuntimeData(BlockTemplateEditData);
            TerrainMaterial = BlockTemplateEditRuntimeData.Material;
        }

        public void RefreshEnvironmentTemplateRuntimeData()
        {
            if (EnvironmentTemplateEditData == null)
                return;
            EnvironmentTemplateEditRuntimeData = new EnvironmentTemplateEditRuntimeData(EnvironmentTemplateEditData);
            EnvironmentTemplateIdMapping.Clear();
            for (int i = 0; i < EnvironmentTemplateEditRuntimeData.CategoryDataList.Count; i++)
            {
                var category = EnvironmentTemplateEditRuntimeData.CategoryDataList[i];
                for (int j= 0; j < category.PrefabList.Count; j++)
                {
                    var prefab = category.PrefabList[i].Prefab.EditorInstance;
                    if (prefab == null)
                        continue;
                    EnvironmentTemplateIdMapping.AddMapping(false, category.CategoryName, prefab.name);
                }
                for (int j = 0; j < category.InstanceMeshDataList.Count; j++)
                {
                    var prefab = category.InstanceMeshDataList[i].OriginObject.EditorInstance;
                    if (prefab == null)
                        continue;
                    EnvironmentTemplateIdMapping.AddMapping(true, category.CategoryName, prefab.name);
                }
            }
        }

        public Vector3Int GetBlockNum()
        {
            return ChunkBlockNum * ChunkNum;
        }

        public Vector3Int GetChunkSize()
        {
            return BlockSize * ChunkBlockNum;
        }

        public Vector3 TerrainSize()
        {
            return BlockSize * ChunkBlockNum * ChunkNum;
        }

        public bool TryGetId(Vector3 position, out int chunkId, out int blockInChunkId)
        {
            var worldBlockCoord = GetWorldBlockCoord(position);
            return TryGetId(worldBlockCoord, out chunkId, out blockInChunkId);
        }

        public bool TryGetId(Vector3Int worldBlockCoord, out int chunkId, out int blockInChunkId)
        {
            chunkId = 0;
            blockInChunkId = 0;
            var chunkCoord = GetChunkCoordWithWorldBlockCoord(worldBlockCoord);
            if (!IsValidChunkCoordinates(chunkCoord))
                return false;
            var blockInChunkCoord = GetBlockInChunkCoordWithWorldBlockCoord(worldBlockCoord);
            if (!IsValidBlockInChunkCoordinates(blockInChunkCoord))
                return false;

            chunkId = GetChunkIdWithCoord(chunkCoord);
            blockInChunkId = GetBlocInChunkIdWithCoord(blockInChunkCoord);
            return true;
        }

        #region Chunk

        #region GetData
        public bool TryGetChunk(Vector3 position, out ChunkEditRuntimeData chunkData, out GetChunkReason reason)
        {
            reason = GetChunkReason.Success;
            chunkData = null;
            if (!TryGetId(position, out var chunkId, out _))
            {
                reason = GetChunkReason.PositionOutOfRange;
                return false;
            }
            if (!IdToChunkEditData.TryGetValue(chunkId, out chunkData))
            {
                reason = GetChunkReason.ChunkNotCreated;
                return false;
            }

            reason = GetChunkReason.Success;
            return true;
        }
        #endregion

        #region ChunkId
        public bool IsValidChunkId(int id)
        {
            return id >= 0 && id < ChunkNum.x * ChunkNum.y * ChunkNum.z;
        }
        public int GetChunkId(Vector3 position)
        {
            var chunkCoord = GetChunkCoordinates(position);
            return GetChunkIdWithCoord(chunkCoord);
        }

        public int GetChunkIdWithCoord(Vector3Int chunkCoord)
        {
            return chunkCoord.x +
                chunkCoord.z * ChunkNum.x +
                chunkCoord.y * (ChunkNum.x * ChunkNum.z);
        }
        #endregion

        #region ChunkCoordinates
        public bool IsValidChunkCoordinates(Vector3Int chunkCoord)
        {
            if (chunkCoord.x < 0 || chunkCoord.x >= ChunkNum.x)
                return false;
            if (chunkCoord.y < 0 || chunkCoord.y >= ChunkNum.y)
                return false;
            if (chunkCoord.z < 0 || chunkCoord.z >= ChunkNum.z)
                return false;
            return true;
        }

        public Vector3Int GetChunkCoordinates(Vector3 position)
        {
            var chunkSize = GetChunkSize();
            return new Vector3Int(
                Mathf.FloorToInt(position.x) / chunkSize.x,
                Mathf.FloorToInt(position.y) / chunkSize.y,
                Mathf.FloorToInt(position.z) / chunkSize.z);
        }

        public Vector3Int GetChunkCoordinatesWithId(int chunkId)
        {
            var xzId = chunkId % (ChunkNum.x * ChunkNum.z);
            var y = chunkId / (ChunkNum.x * ChunkNum.z);
            var z = xzId / ChunkNum.x;
            var x = xzId % ChunkNum.x;
            return new Vector3Int(x, y, z);
        }

        public Vector3Int GetChunkCoordWithWorldBlockCoord(Vector3Int worldBlockCoord)
        {
            return new Vector3Int(
                worldBlockCoord.x / ChunkBlockNum.x,
                worldBlockCoord.y / ChunkBlockNum.y,
                worldBlockCoord.z / ChunkBlockNum.z);
        }
        #endregion

        #region ChunkPivotPosition
        public Vector3 GetChunkPivotPosition(Vector3 position)
        {
            var chunkCoord = GetChunkCoordinates(position);
            return GetChunkPivotPositionWithCoord(chunkCoord);
        }

        public Vector3 GetChunkPivotPositionWithCoord(Vector3Int blockCoord)
        {
            var chunkSize = GetChunkSize();
            return new Vector3(
                blockCoord.x * chunkSize.x,
                blockCoord.y * chunkSize.y,
                blockCoord.z * chunkSize.z);
        }

        public Vector3 GetChunkPivotPositionWithId(int chunkId)
        {
            var chunkCoord = GetChunkCoordinatesWithId(chunkId);
            return GetChunkPivotPositionWithCoord(chunkCoord);
        }
        #endregion

        #region ChunkCenterPosition
        public Vector3 GetChunkCenterPositionWithCoord(Vector3Int chunkCoord)
        {
            var chunkSize = GetChunkSize();
            var centerRedress = new Vector3(
               chunkSize.x / 2f,
               chunkSize.y / 2f,
               chunkSize.z / 2f);
            return GetChunkPivotPositionWithCoord(chunkCoord) + centerRedress;
        }
        public Vector3 GetChunkCenterPositionWithId(int chunkId)
        {
            var chunkSize = GetChunkSize();
            var centerRedress = new Vector3(
               chunkSize.x / 2f,
               chunkSize.y / 2f,
               chunkSize.z / 2f);
            return GetChunkPivotPositionWithId(chunkId) + centerRedress;
        }
        #endregion

        #endregion

        #region Block

        #region Data

        public void AddBlockData(int chunkId, int blockId, int blockTemplateId)
        {
            if (!IsValidChunkCoordinates(GetChunkCoordinatesWithId(chunkId)))
                return;
            if (!IsValidBlockInChunkCoordinates(GetBlockInChunkCoordinatesWithId(blockId)))
                return;

            if (!IdToChunkEditData.TryGetValue(chunkId, out var chunkEditRuntime))
            {
                chunkEditRuntime = new ChunkEditRuntimeData(chunkId);
                IdToChunkEditData.Add(chunkId, chunkEditRuntime);
            }

            if (!chunkEditRuntime.IdToBlockEditData.TryGetValue(blockId, out var blockEditRuntimeData))
            {
                blockEditRuntimeData = new BlockEditRuntimeData(blockId, blockTemplateId);
                chunkEditRuntime.IdToBlockEditData.Add(blockId, blockEditRuntimeData);
            }
        }

        public void AddBlockData(int chunkId, int blockId, int blockTemplateId, Vector4 yTopValue, Vector4 yBottomValue)
        {
            if (!IsValidChunkCoordinates(GetChunkCoordinatesWithId(chunkId)))
                return;
            if (!IsValidBlockInChunkCoordinates(GetBlockInChunkCoordinatesWithId(blockId)))
                return;

            if (!IdToChunkEditData.TryGetValue(chunkId, out var chunkEditRuntime))
            {
                chunkEditRuntime = new ChunkEditRuntimeData(chunkId);
                IdToChunkEditData.Add(chunkId, chunkEditRuntime);
            }

            if (!chunkEditRuntime.IdToBlockEditData.TryGetValue(blockId, out var blockEditRuntimeData))
            {
                blockEditRuntimeData = new BlockEditRuntimeData(blockId, blockTemplateId, yTopValue, yBottomValue);
                chunkEditRuntime.IdToBlockEditData.Add(blockId, blockEditRuntimeData);
            }
        }

        public void UpdateBlockData(int chunkId, int blockId, int blockTemplateId)
        {
            if (!IsValidChunkCoordinates(GetChunkCoordinatesWithId(chunkId)))
                return;
            if (!IsValidBlockInChunkCoordinates(GetBlockInChunkCoordinatesWithId(blockId)))
                return;

            if (!IdToChunkEditData.TryGetValue(chunkId, out var chunkEditRuntime))
                return;

            if (!chunkEditRuntime.IdToBlockEditData.TryGetValue(blockId, out var blockEditRuntimeData))
                return;

            blockEditRuntimeData.TemplateId = blockTemplateId;
        }

        public void RemoveBlockData(int chunkId, int blockId)
        {
            if (!IsValidChunkCoordinates(GetChunkCoordinatesWithId(chunkId)))
                return;
            if (!IsValidBlockInChunkCoordinates(GetBlockInChunkCoordinatesWithId(blockId)))
                return;

            if (!IdToChunkEditData.TryGetValue(chunkId, out var chunkEditRuntime))
                return;

            chunkEditRuntime.IdToBlockEditData.Remove(blockId);
        }

        public bool TryGetBlock(int chunkId, int blockId, out BlockEditRuntimeData blockData, out GetBlockReason reason)
        {
            blockData = null;
            reason = GetBlockReason.Success;
            if (!IsValidChunkId(chunkId) || !IsValidBlockInChunkId(blockId))
            {
                reason = GetBlockReason.IdOutOfRange;
                return false;
            }

            if (!IdToChunkEditData.TryGetValue(chunkId, out var chunk))
            {
                reason = GetBlockReason.ChunkNotCreated;
                return false;
            }

            if (!chunk.IdToBlockEditData.TryGetValue(blockId, out blockData))
            {
                reason = GetBlockReason.BlockNotCreated;
                return false;
            }

            reason = GetBlockReason.Success;
            return true;
        }

        public bool TryGetBlock(Vector3 position, out BlockEditRuntimeData blockData, out GetBlockReason reason)
        {
            reason = GetBlockReason.Success;
            blockData = null;
            if (!TryGetId(position, out var chunkId, out var blockInChunkId))
            {
                reason = GetBlockReason.PositionOutOfRange;
                return false;
            }

            return TryGetBlock(chunkId, blockInChunkId, out blockData, out reason);
        }

        public bool TryGetBlock(Vector3Int worldBlockCoord, out BlockEditRuntimeData blockData, out GetBlockReason reason)
        {
            reason = GetBlockReason.Success;
            blockData = null;
            if (!TryGetId(worldBlockCoord, out var chunkId, out var blockInChunkId))
            {
                reason = GetBlockReason.PositionOutOfRange;
                return false;
            }

            return TryGetBlock(chunkId, blockInChunkId, out blockData, out reason);
        }

        #endregion

        #region BlockInChunkId
        public bool IsValidBlockInChunkId(int id)
        {
            return id >= 0 || id < ChunkBlockNum.x * ChunkBlockNum.y * ChunkBlockNum.z;
        }

        public int GetBlockInChunkId(Vector3 inChunkPosition)
        {
            var blockCoord = GetBlockInChunkCoordinates(inChunkPosition);
            return blockCoord.x +
                blockCoord.z * ChunkBlockNum.x +
                blockCoord.y * (ChunkBlockNum.x * ChunkBlockNum.z);
        }

        public int GetBlocInChunkIdWithCoord(Vector3Int blockIChunkCoord)
        {
            return blockIChunkCoord.x +
                blockIChunkCoord.z * ChunkBlockNum.x +
                blockIChunkCoord.y * (ChunkBlockNum.x * ChunkBlockNum.z);
        }
        #endregion

        #region WorldBlockCoordinates
        public bool IsValidWorldBlockCoordinates(Vector3 worldBlockCoord)
        {
            var blockNum = GetBlockNum();
            if (worldBlockCoord.x < 0 || worldBlockCoord.x >= blockNum.x)
                return false;
            if (worldBlockCoord.y < 0 || worldBlockCoord.y >= blockNum.y)
                return false;
            if (worldBlockCoord.z < 0 || worldBlockCoord.z >= blockNum.z)
                return false;
            return true;
        }

        public Vector3Int GetWorldBlockCoord(Vector3 worldPosition)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPosition.x / BlockSize.x),
                Mathf.FloorToInt(worldPosition.y / BlockSize.y),
                Mathf.FloorToInt(worldPosition.z / BlockSize.z));
        }

        public Vector3Int GetWorldBlockCoordWithId(int chunkId, int blockId)
        {
            var chunkCoord = GetChunkCoordinatesWithId(chunkId);
            var blockInChunkCoord = GetBlockInChunkCoordinatesWithId(blockId);
            return new Vector3Int(
                chunkCoord.x * ChunkBlockNum.x + blockInChunkCoord.x,
                chunkCoord.y * ChunkBlockNum.y + blockInChunkCoord.y,
                chunkCoord.z * ChunkBlockNum.z + blockInChunkCoord.z);
        }
        #endregion

        #region BlockInChunkCoordinates
        public bool IsValidBlockInChunkCoordinates(Vector3Int blockCoord)
        {
            if (blockCoord.x < 0 || blockCoord.x >= ChunkBlockNum.x)
                return false;
            if (blockCoord.y < 0 || blockCoord.y >= ChunkBlockNum.y)
                return false;
            if (blockCoord.z < 0 || blockCoord.z >= ChunkBlockNum.z)
                return false;
            return true;
        }

        public Vector3Int GetBlockInChunkCoordinates(Vector3 inChunkPosition)
        {
            return new Vector3Int(
                Mathf.FloorToInt(inChunkPosition.x / BlockSize.x),
                Mathf.FloorToInt(inChunkPosition.y / BlockSize.y),
                Mathf.FloorToInt(inChunkPosition.z / BlockSize.z));
        }

        public Vector3Int GetBlockInChunkCoordinatesWithId(int blockId)
        {
            var xzId = blockId % (ChunkBlockNum.x * ChunkBlockNum.z);
            var y = blockId / (ChunkBlockNum.x * ChunkBlockNum.z);
            var z = xzId / ChunkBlockNum.x;
            var x = xzId % ChunkBlockNum.x;
            return new Vector3Int(x, y, z);
        }

        public Vector3Int GetBlockInChunkCoordWithWorldBlockCoord(Vector3Int worldBlockCoord)
        {
            return new Vector3Int(
                worldBlockCoord.x % ChunkBlockNum.x,
                worldBlockCoord.y % ChunkBlockNum.y,
                worldBlockCoord.z % ChunkBlockNum.z);
        }
        #endregion

        #region BlockInChunkPivotPosition
        public Vector3 GetBlockInChunkPivotPosition(Vector3 position)
        {
            var blockCoord = GetBlockInChunkCoordinates(position);
            return new Vector3(
                blockCoord.x * BlockSize.x,
                blockCoord.y * BlockSize.y,
                blockCoord.z * BlockSize.z);
        }

        public Vector3 GetBlockInChunkPivotPositionWithCoord(Vector3Int blockInChunkCoord)
        {
            return new Vector3(
                blockInChunkCoord.x * BlockSize.x,
                blockInChunkCoord.y * BlockSize.y,
                blockInChunkCoord.z * BlockSize.z);
        }

        public Vector3 GetBlockInChunkPivotPositionWithId(int blockId)
        {
            var blockCoord = GetBlockInChunkCoordinatesWithId(blockId);
            return GetBlockInChunkPivotPositionWithCoord(blockCoord);
        }
        #endregion

        #region BlockInChunkCenterPosition
        public Vector3 GetBlockInChunkCenterPositionWithCoord(Vector3Int blockInCoord)
        {
            var centerRedress = new Vector3(
               BlockSize.x / 2f,
               BlockSize.y / 2f,
               BlockSize.z / 2f);
            return GetBlockInChunkPivotPositionWithCoord(blockInCoord) + centerRedress;
        }

        public Vector3 GetBlockInChunkCenterPositionWithId(int blockId)
        {
            var blockCoord = GetBlockInChunkCoordinatesWithId(blockId);
            return GetBlockInChunkCenterPositionWithCoord(blockCoord);
        }
        #endregion

        #region WorldBlockPivotPosition
        public Vector3 GetWorldBlockPivotPositionWithId(int chunkId, int blockId)
        {
            var chunkPivotPosition = GetChunkPivotPositionWithId(chunkId);
            var blockPivotPosition = GetBlockInChunkPivotPositionWithId(blockId);
            return chunkPivotPosition + blockPivotPosition;
        }
        #endregion

        #region WorldBlockCenterPosition
        public Vector3 GetWorldBlockCenterPositionWithId(int chunkId, int blockId)
        {
            var chunkPivotPosition = GetChunkPivotPositionWithId(chunkId);
            var blockCenterPosition = GetBlockInChunkCenterPositionWithId(blockId);
            return chunkPivotPosition + blockCenterPosition;
        }
        #endregion

        #endregion

        #region Environment

        public bool AddEnvironment(bool isInsMesh, string categoryName, string name, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (!IsValid(position))
                return false;

            if (!TryGetChunk(position, out var chunkData, out _))
                return false;

            chunkData.AddEnvironment(++EnvironmentInstanceId, isInsMesh, categoryName, name, position, rotation, scale);
            return true;
        }

        private List<int> _removedEnvInstanceIdListCache = new List<int>();
        public List<int> RemoveEnvironment(Vector3 position, float radius, Func<string, string, bool> filter = null)
        {
            _removedEnvInstanceIdListCache.Clear();
            if (!IsValid(position))
                return _removedEnvInstanceIdListCache;

            if (!TryGetChunk(position, out var chunkData, out _))
                return _removedEnvInstanceIdListCache;

            bool haveFilter = filter != null;

            var rangeDis = radius * radius;
            for (int i = chunkData.EnvironmentEditDataList.Count - 1; i >= 0; i--)
            {
                var envData = chunkData.EnvironmentEditDataList[i];
                if (haveFilter && !filter(envData.CategoryName, envData.Name))
                    continue;
                for (int j = envData.InstanceList.Count - 1; j >= 0; j--)
                {
                    var insData = envData.InstanceList[j];
                    var diff = insData.Position - position;
                    var dis = diff.sqrMagnitude;
                    if (dis > rangeDis)
                        continue;

                    envData.InstanceList.RemoveAt(j);
                    _removedEnvInstanceIdListCache.Add(insData.InstanceId);
                }
                if (envData.InstanceList.Count == 0)
                    chunkData.EnvironmentEditDataList.RemoveAt(i);
            }

            return _removedEnvInstanceIdListCache;
        }

        public bool TryGetEnvPrefabData(int id, out EnvironmentTemplatePrefabEditRuntimeData prefabData)
        {
            prefabData = null;
            if (!EnvironmentTemplateIdMapping.TryGetInfo(id, out var info))
                return false;

            return EnvironmentTemplateEditRuntimeData.TryGetPrefab(info.CategoryName, info.Name, out prefabData);
        }

        public bool TryGetEnvInsMeshData(int id, out EnvironmentTemplateInstanceMeshEditRuntimeData insMeshData)
        {
            insMeshData = null;
            if (!EnvironmentTemplateIdMapping.TryGetInfo(id, out var info))
                return false;

            return EnvironmentTemplateEditRuntimeData.TryGetInsMesh(info.CategoryName, info.Name, out insMeshData);
        }

        #endregion

        #region Area

        public void AddAreaGroup()
        {
            var nextId = GetAreaGroupNextId();
            AreaGroupEditDataList.Add(new AreaGroupEditRuntimeData(nextId));
        }

        public bool TryGetAreaGroup(int groupId, out AreaGroupEditRuntimeData data)
        {
            data = null;
            for (int i = 0; i < AreaGroupEditDataList.Count; i++)
            {
                if (AreaGroupEditDataList[i].Id == groupId)
                {
                    data = AreaGroupEditDataList[i];
                    return true;
                }
            }
            return false;
        }

        private int GetAreaGroupNextId()
        {
            var maxId = 0;
            for (int i = 0; i < AreaGroupEditDataList.Count; i++)
            {
                var areaGroup = AreaGroupEditDataList[i];
                if (areaGroup.Id > maxId)
                    maxId = areaGroup.Id;
            }

            return ++maxId;
        }

        #endregion

        public bool IsValid(Vector3 position)
        {
            var size = TerrainSize();
            if (position.x < 0 || position.x >= size.x)
                return false;
            if (position.y < 0 || position.y >= size.y)
                return false;
            if (position.z < 0 || position.z >= size.z)
                return false;
            return true;
        }

        /// <summary>
        /// 發射射線獲取擊中的Block資訊
        /// </summary>
        /// <param name="ray">GUI投向世界的Ray</param>
        /// <param name="distance">最長距離</param>
        /// <param name="chunkNumY">最低限的ChunkY</param>
        /// <param name="result">擊中結果</param>
        /// <returns></returns>
        public bool RaycastBlock(Ray ray, float distance, out RaycastBlockResult result, Func<Vector3Int, RaycastBlockFilterType> filter = null)
        {
            result = default;
            Bounds bounds = new Bounds();
            bounds.SetMinMax(Vector3.zero, TerrainSize());

            // 沒有在在任何Chunk範圍內
            if (!bounds.IntersectRayAABB(ray, distance, out var tDistance))
                return false;

            bool haveFilter = filter != null;
            int lastChunkId = -1;
            int lastBlockId = -1;
            var blockResults = VoxelUtility.RaycastDDA(ray, BlockSize, tDistance, distance);
            foreach (var blockResult in blockResults)
            {
                var worldCoordinates = blockResult.Coordinates;
                if (haveFilter)
                {
                    var filterType = filter.Invoke(worldCoordinates);
                    if (filterType == RaycastBlockFilterType.Continue)
                        continue;
                    else if (filterType == RaycastBlockFilterType.Break)
                        break;
                }
                if (!IsValidWorldBlockCoordinates(worldCoordinates))
                    continue;
                if (!TryGetId(worldCoordinates, out var cId, out var bId))
                    continue;
                if (!TryGetBlock(cId, bId, out var blockData, out var reason)
                    && (reason == GetBlockReason.IdOutOfRange))
                {
                    continue;
                }

                result.WorldBlockCoordinates = worldCoordinates;
                //將射線方向反轉
                result.HitFaceNormal = -blockResult.HitRayNormal;
                result.HitWorldPosition = blockResult.HitWorldPosition;
                result.HaveData = false;
                if (reason == GetBlockReason.ChunkNotCreated
                    || reason == GetBlockReason.BlockNotCreated)
                {
                    lastChunkId = cId;
                    lastBlockId = bId;
                }
                else
                {
                    lastChunkId = cId;
                    lastBlockId = bId;
                    result.HaveData = true;
                    break;
                }
            }

            result.ChunkId = lastChunkId;
            result.BlockId = lastBlockId;
            return result.ChunkId != -1 && result.BlockId != -1;
        }

        private List<RaycastBlockResult> _raycastBlockListCache = new List<RaycastBlockResult>();
        /// <summary>
        /// 發射射線獲取擊中的Block資訊
        /// </summary>
        /// <param name="ray">GUI投向世界的Ray</param>
        /// <param name="distance">最長距離</param>
        /// <param name="chunkNumY">最低限的ChunkY</param>
        /// <param name="result">擊中結果</param>
        /// <returns></returns>
        public bool RaycastBlock(Ray ray, float distance, out List<RaycastBlockResult> result, Func<Vector3Int, RaycastBlockFilterType> filter = null, int maxCount = int.MaxValue)
        {
            result = _raycastBlockListCache;
            result.Clear();
            Bounds bounds = new Bounds();
            bounds.SetMinMax(Vector3.zero, TerrainSize());

            // 沒有在在任何Chunk範圍內
            if (!bounds.IntersectRayAABB(ray, distance, out var tDistance))
                return false;

            bool haveFilter = filter != null;
            var blockResults = VoxelUtility.RaycastDDA(ray, BlockSize, tDistance, distance);
            foreach (var blockResult in blockResults)
            {
                var worldCoordinates = blockResult.Coordinates;
                if (haveFilter)
                {
                    var filterType = filter.Invoke(worldCoordinates);
                    if (filterType == RaycastBlockFilterType.Continue)
                        continue;
                    else if (filterType == RaycastBlockFilterType.Break)
                        break;
                }
                if (!IsValidWorldBlockCoordinates(worldCoordinates))
                    continue;
                if (!TryGetId(worldCoordinates, out var cId, out var bId))
                    continue;
                if (!TryGetBlock(cId, bId, out var blockData, out var reason))
                    continue;

                var hitResult = new RaycastBlockResult();
                hitResult.WorldBlockCoordinates = worldCoordinates;
                hitResult.HitFaceNormal = -blockResult.HitRayNormal;
                hitResult.HitWorldPosition = blockResult.HitWorldPosition;
                hitResult.HaveData = false;
                hitResult.HaveData = true;
                hitResult.ChunkId = cId;
                hitResult.BlockId = bId;
                result.Add(hitResult);
                if (result.Count >= maxCount)
                    break;
            }

            return result.Count != 0;
        }

        public bool RaycastBlockMesh(Ray ray, float distance, out RaycastHit hit)
        {
            hit = default;
            if (!RaycastBlock(ray, distance, out var raycastBlockResultList, null, 8))
                return false;

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var uvs2 = new List<Vector2>();
            var uvs3 = new List<Vector2>();

            for (int i = 0; i < raycastBlockResultList.Count; i++)
            {
                var raycastBlockResult = raycastBlockResultList[i];
                if (!IdToChunkEditData.TryGetValue(raycastBlockResult.ChunkId, out var chunkData))
                    return false;

                if (!chunkData.IdToBlockEditData.TryGetValue(raycastBlockResult.BlockId, out var blockData))
                    return false;

                TerrainEditorUtility.CreateBlock(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    this,
                    chunkData,
                    blockData);
            }

            for (int j = 0; j < triangles.Count; j += 3)
            {
                var result = MathUtils.IntersectRayTriangle(
                    ray,
                    vertices[triangles[j]],
                    vertices[triangles[j + 1]],
                    vertices[triangles[j + 2]],
                    true);
                if (result == null)
                    continue;

                hit = (RaycastHit)result;
                return true;
            }

            return false;
        }
    }
}