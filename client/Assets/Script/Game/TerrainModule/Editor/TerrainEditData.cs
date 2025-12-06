using System;
using System.Collections.Generic;
using UnityEngine;
using static TerrainModule.TerrainDefine;

namespace TerrainModule.Editor
{
    [Serializable]
    public class AreaEditData
    {
        public int Id;
        public AreaType AreaType;

        public Vector3 WorldPoint;
        public float Radius;

        public AreaEditData(AreaEditRuntimeData runtimeData)
        {
            Id = runtimeData.Id;
            AreaType = runtimeData.AreaType;

            WorldPoint = runtimeData.WorldPoint;
            Radius = runtimeData.Radius;
        }
    }

    [Serializable]
    public class AreaGroupEditData
    {
        public int Id;
        public string Description;
        public List<AreaEditData> AreaList = new List<AreaEditData>();

        public AreaGroupEditData(AreaGroupEditRuntimeData runtimeData)
        {
            Id = runtimeData.Id;
            Description = runtimeData.Description;
            for (int i = 0; i < runtimeData.AreaList.Count; i++)
            {
                AreaList.Add(new AreaEditData(runtimeData.AreaList[i]));
            }
        }
    }

    [Serializable]
    public class EnvironmentInstanceEditData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public EnvironmentInstanceEditData(EnvironmentInstanceEditRuntimeData runtimeData)
        {
            Position = runtimeData.Position;
            Rotation = runtimeData.Rotation;
            Scale = runtimeData.Scale;
        }
    }

    [Serializable]
    public class EnvironmentEditData
    {
        public bool IsInstanceMesh;
        public string CategoryName;
        public string Name;

        public List<EnvironmentInstanceEditData> InstanceList = new List<EnvironmentInstanceEditData>();

        public EnvironmentEditData(EnvironmentEditRuntimeData runtimeData)
        {
            IsInstanceMesh = runtimeData.IsInstanceMesh;
            CategoryName = runtimeData.CategoryName;
            Name = runtimeData.Name;
            for (int i = 0; i < runtimeData.InstanceList.Count; i++)
            {
                InstanceList.Add(new EnvironmentInstanceEditData(runtimeData.InstanceList[i]));
            }
        }
    }

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
            TemplateId = runtimeData.TemplateId;

            YTopValue = runtimeData.YTopValue;
            YBottomValue = runtimeData.YBottomValue;
        }
    }

    [Serializable]
    public class ChunkEditData
    {
        public int Id;
        public List<BlockEditData> BlockEditDataList = new List<BlockEditData>();
        public List<EnvironmentEditData> EnvironmentEditDataList = new List<EnvironmentEditData>();

        public ChunkEditData(ChunkEditRuntimeData runtimeData)
        {
            Id = runtimeData.Id;
            BlockEditDataList.Clear();
            foreach (var blockEditData in runtimeData.IdToBlockEditData.Values)
            {
                BlockEditDataList.Add(new BlockEditData(blockEditData));
            }
            for (int i = 0; i < runtimeData.EnvironmentEditDataList.Count; i++)
            {
                EnvironmentEditDataList.Add(new EnvironmentEditData(runtimeData.EnvironmentEditDataList[i]));
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

        public BlockTemplateEditData BlockTemplateEditData;
        public EnvironmentTemplateEditData EnvironmentTemplateEditData;

        public List<ChunkEditData> ChunkEditDataList = new List<ChunkEditData>();
        public List<AreaGroupEditData> AreaGroupEditDataList = new List<AreaGroupEditData>();

        public TerrainEditData(
            Vector3Int blockSize,
            Vector3Int chunkBlockNum,
            Vector3Int chunkNum,
            BlockTemplateEditData blockTemplateEditData,
            EnvironmentTemplateEditData environmentTemplateEditData)
        {
            BlockSize = blockSize;
            ChunkBlockNum = chunkBlockNum;
            ChunkNum = chunkNum;
            BlockTemplateEditData = blockTemplateEditData;
            EnvironmentTemplateEditData = environmentTemplateEditData;
        }

        public TerrainEditData(TerrainEditRuntimeData runtimeData)
        {
            UpdateData(runtimeData);
        }

        public void UpdateData(TerrainEditRuntimeData runtimeData)
        {
            BlockSize = runtimeData.BlockSize;
            ChunkBlockNum = runtimeData.ChunkBlockNum;
            ChunkNum = runtimeData.ChunkNum;
            BlockTemplateEditData = runtimeData.BlockTemplateEditData;
            EnvironmentTemplateEditData = runtimeData.EnvironmentTemplateEditData;

            ChunkEditDataList.Clear();
            foreach (var chunkEditData in runtimeData.IdToChunkEditData.Values)
            {
                ChunkEditDataList.Add(new ChunkEditData(chunkEditData));
            }

            AreaGroupEditDataList.Clear();
            for (var i = 0; i < runtimeData.AreaGroupEditDataList.Count; i++)
            {
                AreaGroupEditDataList.Add(new AreaGroupEditData(runtimeData.AreaGroupEditDataList[i]));
            }
        }
    }
}
