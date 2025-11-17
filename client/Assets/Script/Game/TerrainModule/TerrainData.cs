using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule
{
    [Serializable]
    public class BlockData
    {
        //In Chunk
        public int Id;
        public int TemplateId;
    }

    [Serializable]
    public class ChunkData
    {
        public int Id;
        public string MeshName;
        public List<BlockData> BlockDataList = new List<BlockData>();

        public ChunkData(int id)
        {
            Id = id;
        }
    }

    /// <summary>
    /// 地形靜態資料
    /// </summary>
    public class TerrainData : ScriptableObject
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

        public List<ChunkData> ChunkDataList = new List<ChunkData>();

        public TerrainData(Vector3Int blockSize, Vector3Int chunkBlockNum, Vector3Int chunkNum)
        {
            BlockSize = blockSize;
            ChunkBlockNum = chunkBlockNum;
            ChunkNum = chunkNum;
        }
    }
}