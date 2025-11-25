using Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    [Serializable]
    public class EnvironmentTemplateInstanceMeshData
    {
        public MeshIndirectField Mesh = new MeshIndirectField();
        public MaterialIndirectField Material = new MaterialIndirectField();
    }

    [Serializable]
    public class EnvironmentTemplatePrefabData
    {
        public GameObjectIndirectField Prefab = new GameObjectIndirectField();
    }

    [Serializable]
    public class EnvironmentTemplateCategoryData
    {
        public string CategoryName;
        public List<EnvironmentTemplatePrefabData> PrefabList = new List<EnvironmentTemplatePrefabData>();
        public List<EnvironmentTemplateInstanceMeshData> InstanceMeshDataList = new List<EnvironmentTemplateInstanceMeshData>();
    }

    [CreateAssetMenu(fileName = "EnvironmentTemplateEditData", menuName = "Terrain/En")]
    public class EnvironmentTemplateEditData : ScriptableObject
    {
        public List<EnvironmentTemplateCategoryData> CategoryDataList = new List<EnvironmentTemplateCategoryData>();

        public EnvironmentTemplateEditData()
        {

        }

        public EnvironmentTemplateEditData(EnvironmentTemplateEditRuntimeData runtimeData)
        {
            UpdateData(runtimeData);
        }

        public void UpdateData(EnvironmentTemplateEditRuntimeData runtimeData)
        {

        }
    }
}
