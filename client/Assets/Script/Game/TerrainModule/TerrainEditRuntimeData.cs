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
        ChunkNotCreated,
        BlockNotCreated,
    }

    public enum GetChunkReason
    {
        Success,
        PositionOutOfRange,
        ChunkNotCreated,
    }

    public struct RaycastChunkResult
    {
        public int ChunkId;
        public List<int> BlockIdList;

        public RaycastChunkResult(int id)
        {
            ChunkId = id;
            BlockIdList = new List<int>();
        }
    }

    public class BlockEditRuntimeData
    {
        //In Chunk
        public int Id;

        public BlockEditRuntimeData(BlockEditData editData)
        {
            Id = editData.Id;
        }
    }

    public class ChunkEditRuntimeData
    {
        public int Id;

        public Dictionary<int, BlockEditRuntimeData> IdToBlockEditData = new Dictionary<int, BlockEditRuntimeData>();

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
            chunkId = 0;
            blockInChunkId = 0;
            if (!IsValid(position))
                return false;
            chunkId = GetChunkId(position);
            var chunkPivotPos = GetChunkPivotPosition(position);
            blockInChunkId = GetBlockInChunkId(position - chunkPivotPos);
            return true;
        }

        #region Chunk

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

        public Vector3Int GetChunkCoordinates(Vector3 position)
        {
            var chunkSize = GetChunkSize();
            return new Vector3Int(
                Mathf.FloorToInt(position.x) / chunkSize.x,
                Mathf.FloorToInt(position.y) / chunkSize.y,
                Mathf.FloorToInt(position.z) / chunkSize.z);
        }

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

        public Vector3Int GetChunkCoordinatesWithId(int chunkId)
        {
            var xzId = chunkId % (ChunkNum.x * ChunkNum.z);
            var y = chunkId / (ChunkNum.x * ChunkNum.z);
            var z = xzId / ChunkNum.x;
            var x = xzId % ChunkNum.x;
            return new Vector3Int(x, y, z);
        }

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

        #region Block

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

        public int GetBlockInChunkId(Vector3 inChunkPosition)
        {
            var blockCoord = GetBlockCoordinates(inChunkPosition);
            return blockCoord.x +
                blockCoord.z * ChunkBlockNum.x +
                blockCoord.y * (ChunkBlockNum.x * ChunkBlockNum.z);
        }

        public int GetBlockIdWithCoord(Vector3Int blockCoord)
        {
            return blockCoord.x +
                blockCoord.z * ChunkBlockNum.x +
                blockCoord.y * (ChunkBlockNum.x * ChunkBlockNum.z);
        }

        public bool IsValidBlockCoordinates(Vector3Int blockCoord)
        {
            if (blockCoord.x < 0 || blockCoord.x >= ChunkBlockNum.x)
                return false;
            if (blockCoord.y < 0 || blockCoord.y >= ChunkBlockNum.y)
                return false;
            if (blockCoord.z < 0 || blockCoord.z >= ChunkBlockNum.z)
                return false;
            return true;
        }

        public Vector3Int GetBlockCoordinates(Vector3 inChunkPosition)
        {
            return new Vector3Int(
                Mathf.FloorToInt(inChunkPosition.x / BlockSize.x),
                Mathf.FloorToInt(inChunkPosition.y / BlockSize.y),
                Mathf.FloorToInt(inChunkPosition.z / BlockSize.z));
        }

        public Vector3Int GetBlockCoordinatesWithId(int blockId)
        {
            var xzId = blockId % (ChunkBlockNum.x * ChunkBlockNum.z);
            var y = blockId / (ChunkBlockNum.x * ChunkBlockNum.z);
            var z = xzId / ChunkBlockNum.x;
            var x = xzId % ChunkBlockNum.x;
            return new Vector3Int(x, y, z);
        }

        public Vector3 GetBlockPivotPosition(Vector3 position)
        {
            var blockCoord = GetBlockCoordinates(position);
            return new Vector3(
                blockCoord.x * BlockSize.x,
                blockCoord.y * BlockSize.y,
                blockCoord.z * BlockSize.z);
        }

        public Vector3 GetBlockPivotPositionWithCoord(Vector3Int blockCoord)
        {
            return new Vector3(
                blockCoord.x * BlockSize.x,
                blockCoord.y * BlockSize.y,
                blockCoord.z * BlockSize.z);
        }

        public Vector3 GetBlockCenterPositionWithCoord(Vector3Int blockCoord)
        {
            var centerRedress = new Vector3(
               BlockSize.x / 2f,
               BlockSize.y / 2f,
               BlockSize.z / 2f);
            return GetBlockPivotPositionWithCoord(blockCoord) + centerRedress;
        }

        public Vector3 GetBlockCenterPositionWithId(int blockId)
        {
            var blockCoord = GetBlockCoordinatesWithId(blockId);
            return GetBlockCenterPositionWithCoord(blockCoord);
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

        public bool Raycast(Ray ray, float distance, int chunkNumY, out List<RaycastChunkResult> chunkIds)
        {
            chunkIds = new List<RaycastChunkResult>();
            Bounds bounds = new Bounds();
            bounds.SetMinMax(Vector3.zero, TerrainSize());

            // 沒有在在任何Chunk範圍內
            if (!bounds.IntersectRayAABB(ray, distance, out var tDistance))
                return false;

            var chunkResults = VoxelUtility.RaycastDDA(ray, GetChunkSize(), tDistance, distance);
            foreach ( var chunkResult in chunkResults)
            {
                var chunkCoord = chunkResult.Coordinates;
                if (!IsValidChunkCoordinates(chunkCoord))
                    continue;
                if (chunkCoord.y < chunkNumY)
                    break;
                var chunkCenterPosition = GetChunkCenterPositionWithCoord(chunkResult.Coordinates);
                if (!TryGetChunk(chunkCenterPosition, out var chunkData, out var chunkReason)
                    && chunkReason == GetChunkReason.PositionOutOfRange)
                {
                    continue;
                }

                int chunkId = -1;
                if (chunkReason == GetChunkReason.ChunkNotCreated)
                {
                    chunkId = GetChunkIdWithCoord(chunkCoord);
                }
                else
                {
                    chunkId = chunkData.Id;
                }
                var rayChunkResult = new RaycastChunkResult(chunkId);
                chunkIds.Add(rayChunkResult);

                //將座標關係轉換成chunk內的
                var chunkPivotPosition = GetChunkPivotPositionWithCoord(chunkCoord);
                var chunkRay = new Ray(ray.origin - chunkPivotPosition, ray.direction);
                var blockResults = VoxelUtility.RaycastDDA(chunkRay, BlockSize, chunkResult.Distance, distance);
                foreach (var blockResult in blockResults)
                {
                    var blockCoord = blockResult.Coordinates;
                    if (!IsValidBlockCoordinates(blockCoord))
                        continue;
                    var blockCenterPosition = GetBlockCenterPositionWithCoord(blockCoord) + chunkPivotPosition;
                    if (!TryGetBlock(blockCenterPosition, out var blockData, out var blockReason)
                        && blockReason == GetBlockReason.PositionOutOfRange)
                    {
                        continue;
                    }

                    if (blockReason == GetBlockReason.ChunkNotCreated || blockReason == GetBlockReason.BlockNotCreated)
                    {
                        rayChunkResult.BlockIdList.Add(GetBlockIdWithCoord(blockCoord));
                    }
                    else
                    {
                        rayChunkResult.BlockIdList.Add(blockData.Id);
                    }
                }
            }
            return chunkIds.Count > 0;
        }

        /// <summary>
        /// 發射射線獲取擊中的Chunk與內部的Block
        /// </summary>
        /// <param name="ray">GUI投向世界的Ray</param>
        /// <param name="distance">最長距離</param>
        /// <param name="chunkNumY">最低限的ChunkY</param>
        /// <param name="chunkId">偵測到的ChunkId</param>
        /// <param name="blockId">偵測到的BlockId</param>
        /// <param name="hitFaceNormal">投射的射線是透過哪個面擊中Block的</param>
        /// <returns></returns>
        public bool Raycast(Ray ray, float distance, int chunkNumY, out int chunkId, out int blockId, out Vector3 hitFaceNormal)
        {
            chunkId = -1;
            blockId = -1;
            hitFaceNormal = Vector3.zero;
            Bounds bounds = new Bounds();
            bounds.SetMinMax(Vector3.zero, TerrainSize());

            // 沒有在在任何Chunk範圍內
            if (!bounds.IntersectRayAABB(ray, distance, out var tDistance))
                return false;

            int lastChunkId = -1;
            int lastBlockId = -1;
            var chunkResults = VoxelUtility.RaycastDDA(ray, GetChunkSize(), tDistance, distance);
            foreach (var chunkResult in chunkResults)
            {
                var chunkCoord = chunkResult.Coordinates;
                if (!IsValidChunkCoordinates(chunkCoord))
                    continue;
                if (chunkCoord.y < chunkNumY)
                    break;
                var chunkCenterPosition = GetChunkCenterPositionWithCoord(chunkResult.Coordinates);
                if (!TryGetChunk(chunkCenterPosition, out var chunkData, out var chunkReason)
                    && chunkReason == GetChunkReason.PositionOutOfRange)
                {
                    continue;
                }

                if (chunkReason == GetChunkReason.ChunkNotCreated)
                {
                    lastChunkId = GetChunkIdWithCoord(chunkCoord);
                }
                else
                {
                    lastChunkId = chunkData.Id;
                }

                //將座標關係轉換成chunk內的
                var chunkPivotPosition = GetChunkPivotPositionWithCoord(chunkCoord);
                var chunkRay = new Ray(ray.origin - chunkPivotPosition, ray.direction);
                var blockResults = VoxelUtility.RaycastDDA(chunkRay, BlockSize, chunkResult.Distance, distance);
                foreach (var blockResult in blockResults)
                {
                    var blockCoord = blockResult.Coordinates;
                    if (!IsValidBlockCoordinates(blockCoord))
                        continue;
                    var blockCenterPosition = GetBlockCenterPositionWithCoord(blockCoord) + chunkPivotPosition;
                    if (!TryGetBlock(blockCenterPosition, out var blockData, out var blockReason)
                        && blockReason == GetBlockReason.PositionOutOfRange)
                    {
                        continue;
                    }

                    hitFaceNormal = blockResult.HitFaceNormal;
                    if (blockReason == GetBlockReason.ChunkNotCreated || blockReason == GetBlockReason.BlockNotCreated)
                    {
                        lastBlockId = GetBlockIdWithCoord(blockCoord);
                    }
                    else
                    {
                        lastBlockId = blockData.Id;
                        break;
                    }
                }
            }
            chunkId = lastChunkId;
            blockId = lastBlockId;
            return chunkId != -1 && blockId != -1;
        }
    }
}