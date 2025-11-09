using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    /// <summary>
    /// 編輯器運行時的資料
    /// </summary>
    public class TerrainEditRuntimeData
    {
        public string Name;

        public Vector3Int BlockSize = Vector3Int.one;

        public Vector3Int ChunkBlockNum = Vector3Int.one;
        public Vector3Int ChunkNum = Vector3Int.one;

        public TerrainEditRuntimeData(TerrainEditData editData)
        {
            Name = editData.name;
            BlockSize = editData.BlockSize;
            ChunkBlockNum = editData.ChunkBlockNum;
            ChunkNum = editData.ChunkNum;
        }

        public Vector3Int GetBlockNum()
        {
            return BlockSize * ChunkBlockNum * ChunkNum;
        }

        public Vector3Int GetChunkSize()
        {
            return BlockSize * ChunkBlockNum;
        }
    }
}