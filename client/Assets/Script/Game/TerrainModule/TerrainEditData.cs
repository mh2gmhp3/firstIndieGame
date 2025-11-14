using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    [Serializable]
    public class BlockEditData
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

        public BlockEditData(BlockEditRuntimeData runtimeData)
        {
            Id = runtimeData.Id;

            YTopValue = runtimeData.YTopValue;
            YBottomValue = runtimeData.YBottomValue;
        }
    }

    [Serializable]
    public class ChunkEditData
    {
        public int Id;
        public List<BlockEditData> BlockEditDataList = new List<BlockEditData>();

        public ChunkEditData(ChunkEditRuntimeData runtimeData)
        {
            Id = runtimeData.Id;
            BlockEditDataList.Clear();
            foreach (var blockEditData in runtimeData.IdToBlockEditData.Values)
            {
                BlockEditDataList.Add(new BlockEditData(blockEditData));
            }
        }
    }

    /// <summary>
    /// 地形靜態編輯資料
    /// </summary>
    public class TerrainEditData : ScriptableObject
    {
        /// <summary>
        /// Block大小
        /// </summary>
        public Vector3Int BlockSize = Vector3Int.one;

        /// <summary>
        /// 一個Chunk有多少個Block
        /// </summary>
        public Vector3Int ChunkBlockNum = Vector3Int.one;
        /// <summary>
        /// 有多少Chunk
        /// </summary>
        public Vector3Int ChunkNum = Vector3Int.one;

        public Material TerrainMaterial;

        public List<ChunkEditData> ChunkEditDataList = new List<ChunkEditData>();

        public TerrainEditData(Vector3Int blockSize, Vector3Int chunkBlockNum, Vector3Int chunkNum, Material terrainMat)
        {
            BlockSize = blockSize;
            ChunkBlockNum = chunkBlockNum;
            ChunkNum = chunkNum;
            TerrainMaterial = terrainMat;
        }

        public TerrainEditData(TerrainEditRuntimeData runtimeData)
        {
            BlockSize = runtimeData.BlockSize;
            ChunkBlockNum = runtimeData.ChunkBlockNum;
            ChunkNum = runtimeData.ChunkNum;

            ChunkEditDataList.Clear();
            foreach (var chunkEditData in runtimeData.IdToChunkEditData.Values)
            {
                ChunkEditDataList.Add(new ChunkEditData(chunkEditData));
            }
            TerrainMaterial = runtimeData.TerrainMaterial;
        }
    }
}
