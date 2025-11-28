using Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    [Serializable]
    public class EnvironmentTemplateInstanceMeshEditSingleData
    {
        public MeshIndirectField Mesh = new MeshIndirectField();
        public MaterialIndirectField Material = new MaterialIndirectField();

        public EnvironmentTemplateInstanceMeshEditSingleData(EnvironmentTemplateInstanceMeshEditRuntimeSingleData runtimeData)
        {
            runtimeData.Mesh.CopyTo(Mesh);
            runtimeData.Material.CopyTo(Material);
        }
    }

    [Serializable]
    public class EnvironmentTemplateInstanceMeshEditData
    {
        public GameObjectIndirectField OriginObject = new GameObjectIndirectField();
        public List<EnvironmentTemplateInstanceMeshEditSingleData> MeshSingleDataList = new List<EnvironmentTemplateInstanceMeshEditSingleData>();

        public EnvironmentTemplateInstanceMeshEditData(EnvironmentTemplateInstanceMeshEditRuntimeData runtimeData)
        {
            runtimeData.OriginObject.CopyTo(OriginObject);
            for (int i = 0; i < runtimeData.MeshSingleDataList.Count; i++)
            {
                MeshSingleDataList.Add(new EnvironmentTemplateInstanceMeshEditSingleData(runtimeData.MeshSingleDataList[i]));
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
