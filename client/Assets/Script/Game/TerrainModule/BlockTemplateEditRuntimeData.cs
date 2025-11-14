using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    [Serializable]
    public class BlockTemplateRuntimeData
    {
        public int Id;

        public Vector2 PXTiling;
        public Vector2 NXTiling;

        public Vector2 PYTiling;
        public Vector2 NYTiling;

        public Vector2 PZTiling;
        public Vector2 NZTiling;

        public BlockTemplateRuntimeData(BlockTemplateData data)
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

    public class BlockTemplateEditRuntimeData
    {
        public string Name;

        public List<BlockTemplateRuntimeData> BlockTemplateDataList = new List<BlockTemplateRuntimeData>();

        public BlockTemplateEditRuntimeData(BlockTemplateEditData editData)
        {
            Name = editData.name;
            BlockTemplateDataList.Clear();
            for (int i = 0; i< editData.BlockTemplateDataList.Count; i++)
            {
                BlockTemplateDataList.Add(new BlockTemplateRuntimeData(editData.BlockTemplateDataList[i]));
            }
        }

        public bool TryGetBlockData(int id, out BlockTemplateRuntimeData result)
        {
            for (int i = 0; i < BlockTemplateDataList.Count; i++)
            {
                if (BlockTemplateDataList[i].Id == id)
                {
                    result = BlockTemplateDataList[i];
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}
