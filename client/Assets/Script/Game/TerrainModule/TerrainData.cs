using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using static TerrainModule.TerrainDefine;

namespace TerrainModule
{
    [Serializable]
    public class AreaData
    {
        public int Id;
        public AreaType AreaType;

        public Vector3 WorldPoint;
        public float Radius;

        public AreaData(int id, AreaType areaType, Vector3 worldPoint, float radius)
        {
            Id = id;
            AreaType = areaType;

            WorldPoint = worldPoint;
            Radius = radius;
        }
    }

    [Serializable]
    public class AreaGroupData
    {
        public int Id;
        public List<AreaData> AreaDataList = new List<AreaData>();

        public AreaGroupData(int id)
        {
            Id = id;
        }
    }

    [Serializable]
    public class ChunkEnvironmentInstanceData
    {
        public int InstanceId;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public ChunkEnvironmentInstanceData(int instanceId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            InstanceId = instanceId;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }

    [Serializable]
    public class ChunkEnvironmentData
    {
        public bool IsInstanceMesh;
        public int Id;
        public List<ChunkEnvironmentInstanceData> InstanceList = new List<ChunkEnvironmentInstanceData>();

        public ChunkEnvironmentData(bool isInsMesh, int id)
        {
            IsInstanceMesh = isInsMesh;
            Id = id;
        }
    }

    [Serializable]
    public class EnvironmentColliderData
    {
        public ColliderType ColliderType;
        public Vector3 Center;
        public Vector3 Size;
        public float Radius;
        public float Height;
        public int Direction;

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public EnvironmentColliderData(
            ColliderType colliderType,
            Vector3 center,
            Vector3 size,
            float radius,
            float height,
            int direction,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale)
        {
            ColliderType = colliderType;
            Center = center;
            Size = size;
            Radius = radius;
            Height = height;
            Direction = direction;

            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }

    [Serializable]
    public class EnvironmentPrefabData
    {
        public int Id;
        public GameObjectIndirectField Prefab = new GameObjectIndirectField();

        public EnvironmentPrefabData(int id, GameObjectIndirectField prefab)
        {
            Id = id;
            prefab.CopyTo(Prefab);
        }
    }

    [Serializable]
    public class EnvironmentInstanceMeshSingleData
    {
        public MeshIndirectField Mesh = new MeshIndirectField();
        public MaterialIndirectField Material = new MaterialIndirectField();
        public Matrix4x4 Matrix;

        public EnvironmentInstanceMeshSingleData(MeshIndirectField mesh, MaterialIndirectField material, Matrix4x4 matrix)
        {
            mesh.CopyTo(Mesh);
            material.CopyTo(Material);
            Matrix = matrix;
        }
    }

    [Serializable]
    public class EnvironmentInstanceMeshData
    {
        public int Id;
        public List<EnvironmentInstanceMeshSingleData> MeshSingleDataList = new List<EnvironmentInstanceMeshSingleData>();
        public List<EnvironmentColliderData> ColliderDataList = new List<EnvironmentColliderData>();

        public EnvironmentInstanceMeshData(int id)
        {
            Id = id;
        }
    }

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
        public MeshIndirectField Mesh = new MeshIndirectField();
        public List<BlockData> BlockDataList = new List<BlockData>();
        public List<ChunkEnvironmentData> EnvironmentDataList = new List<ChunkEnvironmentData>();

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

        public List<EnvironmentPrefabData> EnvironmentPrefabDataList = new List<EnvironmentPrefabData>();
        public List<EnvironmentInstanceMeshData> EnvironmentInstanceMeshDataList = new List<EnvironmentInstanceMeshData>();
        public List<AreaGroupData> AreaGroupDataList = new List<AreaGroupData>();

        public TerrainData(Vector3Int blockSize, Vector3Int chunkBlockNum, Vector3Int chunkNum)
        {
            BlockSize = blockSize;
            ChunkBlockNum = chunkBlockNum;
            ChunkNum = chunkNum;
        }

        public EnvironmentPrefabData GetEnvPrefabData(int id)
        {
            for (int i = 0; i < EnvironmentPrefabDataList.Count; i++)
            {
                if (EnvironmentPrefabDataList[i].Id == id)
                    return EnvironmentPrefabDataList[i];
            }
            return null;
        }

        public EnvironmentInstanceMeshData GetEnvInsMeshData(int id)
        {
            for (int i = 0; i < EnvironmentInstanceMeshDataList.Count; i++)
            {
                if (EnvironmentInstanceMeshDataList[i].Id == id)
                    return EnvironmentInstanceMeshDataList[i];
            }
            return null;
        }

        public AreaData GetAreaData(int groupId, int id)
        {
            for (int i = 0; i < AreaGroupDataList.Count; i++)
            {
                var areaGroupData = AreaGroupDataList[i];
                if (areaGroupData.Id != groupId)
                    continue;

                for (int j = 0; j < areaGroupData.AreaDataList.Count; j++)
                {
                    var areaData = areaGroupData.AreaDataList[j];
                    if (areaData.Id == id)
                        return areaData;
                }
            }
            return null;
        }
    }
}