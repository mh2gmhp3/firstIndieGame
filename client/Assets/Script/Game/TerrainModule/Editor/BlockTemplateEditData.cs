using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TerrainModule.Editor
{
    [Serializable]
    public class BlockTemplateData
    {
        public int Id;

        //只考慮UV的Tiling Rotation在編輯時直接設定在格子上
        public Vector2 PXTiling;
        public Vector2 NXTiling;

        public Vector2 PYTiling;
        public Vector2 NYTiling;

        public Vector2 PZTiling;
        public Vector2 NZTiling;

        public BlockTemplateData(BlockTemplateRuntimeData data)
        {
            Id = data.Id;
            PXTiling = data.PXTiling;
            NXTiling = data.NXTiling;
            PYTiling = data.PYTiling;
            NYTiling = data.NYTiling;
            PZTiling = data.PZTiling;
            NZTiling = data.NZTiling;
        }
    }

    public class BlockTemplateEditData : ScriptableObject
    {
        public Texture2D TileMap;
        public Vector2 Tiling;
        public Shader Shader;

        public List<BlockTemplateData> BlockTemplateDataList = new List<BlockTemplateData>();

        public BlockTemplateEditData(BlockTemplateEditRuntimeData runtimeData)
        {
            Update(runtimeData);
        }

        public void Update(BlockTemplateEditRuntimeData runtimeData)
        {
            BlockTemplateDataList.Clear();
            for (int i = 0; i < runtimeData.BlockTemplateDataList.Count; i++)
            {
                BlockTemplateDataList.Add(new BlockTemplateData(runtimeData.BlockTemplateDataList[i]));
            }

            TileMap = runtimeData.TileMap;
            Tiling = runtimeData.Tiling;
            Shader = runtimeData.Shader;
        }

        public BlockTemplateEditData(Texture2D tileMap, Vector2 tiling, Shader shader)
        {
            TileMap = tileMap; ;
            Tiling = tiling;
            Shader = shader;
        }
    }
}
