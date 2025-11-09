using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    [Serializable]
    public class BlockEditData
    {

    }

    [Serializable]
    public class ChunkEditData
    {

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

        public TerrainEditData()
        {

        }

        public TerrainEditData(TerrainEditRuntimeData runtimeData)
        {
            BlockSize = runtimeData.BlockSize;
            ChunkBlockNum = runtimeData.ChunkBlockNum;
            ChunkNum = runtimeData.ChunkNum;
        }
    }
}
