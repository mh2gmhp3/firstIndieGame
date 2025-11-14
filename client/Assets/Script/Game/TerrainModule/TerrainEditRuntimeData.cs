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
        /// <summary>
        /// 被射線檢測到的面方向
        /// </summary>
        public Vector3Int HitFaceNormal;
        public bool HaveData;
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
        public float ZRotation;

        public BlockEditRuntimeData(int id)
        {
            Id = id;

            YTopValue = Vector4.one;
            YBottomValue = Vector4.zero;
        }

        public BlockEditRuntimeData(int id, Vector4 yTopValue, Vector4 yBottomValue)
        {
            Id = id;

            SetYValue(yTopValue, yBottomValue);
        }

        public BlockEditRuntimeData(BlockEditData editData)
        {
            Id = editData.Id;

            YTopValue = editData.YTopValue;
            YBottomValue = editData.YBottomValue;
        }

        public void SetYValue(Vector4 topValue, Vector4 bottomValue)
        {
            topValue = new Vector4(
                Mathf.Clamp01(topValue.x),
                Mathf.Clamp01(topValue.y),
                Mathf.Clamp01(topValue.z),
                Mathf.Clamp01(topValue.w));
            bottomValue = new Vector4(
                Mathf.Clamp01(bottomValue.x),
                Mathf.Clamp01(bottomValue.y),
                Mathf.Clamp01(bottomValue.z),
                Mathf.Clamp01(bottomValue.w));
            YTopValue = new Vector4(
                (float)Math.Round((double)Mathf.Clamp(topValue.x, bottomValue.x, 1), 1),
                (float)Math.Round((double)Mathf.Clamp(topValue.y, bottomValue.y, 1), 1),
                (float)Math.Round((double)Mathf.Clamp(topValue.z, bottomValue.z, 1), 1),
                (float)Math.Round((double)Mathf.Clamp(topValue.w, bottomValue.w, 1), 1));
            YBottomValue = new Vector4(
                (float)Math.Round((double)Mathf.Clamp(bottomValue.x, 0, topValue.x), 1),
                (float)Math.Round((double)Mathf.Clamp(bottomValue.y, 0, topValue.y), 1),
                (float)Math.Round((double)Mathf.Clamp(bottomValue.z, 0, topValue.z), 1),
                (float)Math.Round((double)Mathf.Clamp(bottomValue.w, 0, topValue.w), 1));
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

        public Material TerrainMaterial;

        public Dictionary<int, ChunkEditRuntimeData> IdToChunkEditData = new Dictionary<int, ChunkEditRuntimeData>();

        public TerrainEditRuntimeData(TerrainEditData editData)
        {
            Name = editData.name;
            BlockSize = editData.BlockSize;
            ChunkBlockNum = editData.ChunkBlockNum;
            ChunkNum = editData.ChunkNum;
            TerrainMaterial = editData.TerrainMaterial;

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

        public void AddBlockData(int chunkId, int blockId, Vector4 yTopValue, Vector4 yBottomValue)
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
                blockEditRuntimeData = new BlockEditRuntimeData(blockId, yTopValue, yBottomValue);
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
                //將射線方向反轉
                result.HitFaceNormal = -blockResult.HitRayNormal;
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
    }
}