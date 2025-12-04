using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using static TerrainModule.TerrainDefine;

namespace TerrainModule.Editor
{
    [Serializable]
    public class EnvironmentTemplateColliderEditData
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

        public EnvironmentTemplateColliderEditData(EnvironmentTemplateColliderEditRuntimeData runtimeData)
        {
            ColliderType = runtimeData.ColliderType;
            Center = runtimeData.Center;
            Size = runtimeData.Size;
            Radius = runtimeData.Radius;
            Height = runtimeData.Height;
            Direction = runtimeData.Direction;

            Position = runtimeData.Position;
            Rotation = runtimeData.Rotation;
            Scale = runtimeData.Scale;
        }
    }

    [Serializable]
    public class EnvironmentTemplateInstanceMeshEditSingleData
    {
        public MeshIndirectField Mesh = new MeshIndirectField();
        public MaterialIndirectField Material = new MaterialIndirectField();
        public Matrix4x4 Matrix;

        public EnvironmentTemplateInstanceMeshEditSingleData(EnvironmentTemplateInstanceMeshEditRuntimeSingleData runtimeData)
        {
            runtimeData.Mesh.CopyTo(Mesh);
            runtimeData.Material.CopyTo(Material);
            Matrix = runtimeData.Matrix;
        }
    }

    [Serializable]
    public class EnvironmentTemplateInstanceMeshEditData
    {
        public GameObjectIndirectField OriginObject = new GameObjectIndirectField();
        public List<EnvironmentTemplateInstanceMeshEditSingleData> MeshSingleDataList = new List<EnvironmentTemplateInstanceMeshEditSingleData>();
        public List<EnvironmentTemplateColliderEditData> ColliderDataList = new List<EnvironmentTemplateColliderEditData>();

        public EnvironmentTemplateInstanceMeshEditData(EnvironmentTemplateInstanceMeshEditRuntimeData runtimeData)
        {
            runtimeData.OriginObject.CopyTo(OriginObject);
            for (int i = 0; i < runtimeData.MeshSingleDataList.Count; i++)
            {
                MeshSingleDataList.Add(new EnvironmentTemplateInstanceMeshEditSingleData(runtimeData.MeshSingleDataList[i]));
            }
            for (int i = 0; i < runtimeData.ColliderDataList.Count; i++)
            {
                ColliderDataList.Add(new EnvironmentTemplateColliderEditData(runtimeData.ColliderDataList[i]));
            }
        }
    }

    [Serializable]
    public class EnvironmentTemplatePrefabEditData
    {
        public GameObjectIndirectField Prefab = new GameObjectIndirectField();

        public EnvironmentTemplatePrefabEditData(EnvironmentTemplatePrefabEditRuntimeData runtimeData)
        {
            runtimeData.Prefab.CopyTo(Prefab);
        }
    }

    [Serializable]
    public class EnvironmentTemplateCategoryEditData
    {
        public string CategoryName;
        public List<EnvironmentTemplatePrefabEditData> PrefabList = new List<EnvironmentTemplatePrefabEditData>();
        public List<EnvironmentTemplateInstanceMeshEditData> InstanceMeshDataList = new List<EnvironmentTemplateInstanceMeshEditData>();

        public EnvironmentTemplateCategoryEditData(EnvironmentTemplateCategoryEditRuntimeData runtimeData)
        {
            CategoryName = runtimeData.CategoryName;
            for (int i = 0; i < runtimeData.PrefabList.Count; i++)
            {
                PrefabList.Add(new EnvironmentTemplatePrefabEditData(runtimeData.PrefabList[i]));
            }
            for (int i = 0; i < runtimeData.InstanceMeshDataList.Count; i++)
            {
                InstanceMeshDataList.Add(new EnvironmentTemplateInstanceMeshEditData(runtimeData.InstanceMeshDataList[i]));
            }
        }
    }

    [CreateAssetMenu(fileName = "EnvironmentTemplateEditData", menuName = "Terrain/En")]
    public class EnvironmentTemplateEditData : ScriptableObject
    {
        public List<EnvironmentTemplateCategoryEditData> CategoryDataList = new List<EnvironmentTemplateCategoryEditData>();

        public EnvironmentTemplateEditData()
        {

        }

        public EnvironmentTemplateEditData(EnvironmentTemplateEditRuntimeData runtimeData)
        {
            UpdateData(runtimeData);
        }

        public void UpdateData(EnvironmentTemplateEditRuntimeData runtimeData)
        {
            CategoryDataList.Clear();
            for (int i = 0; i < runtimeData.CategoryDataList.Count; i++)
            {
                CategoryDataList.Add(new EnvironmentTemplateCategoryEditData(runtimeData.CategoryDataList[i]));
            }
        }
    }
}
