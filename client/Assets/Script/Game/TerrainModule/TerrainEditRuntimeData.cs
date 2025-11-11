using Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utility;

namespace TerrainModule.Editor
{
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
        public Vector3Int HitFaceNormal;
    }

    public class BlockEditRuntimeData
    {
        //In Chunk
        public int Id;

        public BlockEditRuntimeData(int id)
        {
            Id = id;
        }

        public BlockEditRuntimeData(BlockEditData editData)
        {
            Id = editData.Id;
        }
    }

    public class ChunkEditRuntimeData
    {
        public int Id;

        public Dictionary<int, BlockEditRuntimeData> IdToBlockEditData = new Dictionary<int, BlockEditRuntimeData>();

        public ChunkEditRuntimeData(int id)
        {
            Id = id;
        }

        public ChunkEditRuntimeData(ChunkEditData editData)
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

        public Dictionary<int, ChunkEditRuntimeData> IdToChunkEditData = new Dictionary<int, ChunkEditRuntimeData>();

        public TerrainEditRuntimeData(TerrainEditData editData)
        {
            Name = editData.name;
            BlockSize = editData.BlockSize;
            ChunkBlockNum = editData.ChunkBlockNum;
            ChunkNum = editData.ChunkNum;

            IdToChunkEditData.Clear();
            for (int i = 0; i < editData.ChunkEditDataList.Count; i++)
            {
                var chunk = editData.ChunkEditDataList[i];
                if (chunk == null)
                    continue;

                var chunkRuntimeData = new ChunkEditRuntimeData(chunk);
                IdToChunkEditData.Add(chunkRuntimeData.Id, chunkRuntimeData);
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
        #endregion

        #endregion

        #region Block

        #region GetData
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
            if (!IdToChunkEditData.TryGetValue(chunkId, out var chunk))
            {
                reason = GetBlockReason.ChunkNotCreated;
                return false;
            }

            if (!chunk.IdToBlockEditData.TryGetValue(blockInChunkId, out blockData))
            {
                reason = GetBlockReason.BlockNotCreated;
                return false;
            }

            reason = GetBlockReason.Success;
            return true;
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

        public void AddBlockData(int chunkId, int blockId)
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
                blockEditRuntimeData = new BlockEditRuntimeData(blockId);
                chunkEditRuntime.IdToBlockEditData.Add(blockId, blockEditRuntimeData);
            }
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

        /// <summary>
        /// 發射射線獲取擊中的Block資訊
        /// </summary>
        /// <param name="ray">GUI投向世界的Ray</param>
        /// <param name="distance">最長距離</param>
        /// <param name="chunkNumY">最低限的ChunkY</param>
        /// <param name="result">擊中結果</param>
        /// <returns></returns>
        public bool RaycastBlock(Ray ray, float distance, int chunkNumY, out RaycastBlockResult result)
        {
            result = default;
            Bounds bounds = new Bounds();
            bounds.SetMinMax(Vector3.zero, TerrainSize());

            // 沒有在在任何Chunk範圍內
            if (!bounds.IntersectRayAABB(ray, distance, out var tDistance))
                return false;

            int lastChunkId = -1;
            int lastBlockId = -1;
            var blockResults = VoxelUtility.RaycastDDA(ray, BlockSize, tDistance, distance);
            foreach (var blockResult in blockResults)
            {
                var worldCoordinates = blockResult.Coordinates;
                var chunkCoord = GetChunkCoordWithWorldBlockCoord(worldCoordinates);
                if (chunkCoord.y < chunkNumY)
                    break;
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
                result.HitFaceNormal = blockResult.HitFaceNormal;
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
                    break;
                }
            }

            result.ChunkId = lastChunkId;
            result.BlockId = lastBlockId;
            return result.ChunkId != -1 && result.BlockId != -1;
        }
    }
}