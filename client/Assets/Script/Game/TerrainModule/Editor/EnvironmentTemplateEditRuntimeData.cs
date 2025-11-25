using System;
using System.Collections.Generic;
using Utility;

namespace TerrainModule.Editor
{
    [Serializable]
    public class EnvironmentTemplateInstanceMeshRuntimeData
    {
        public MeshIndirectField Mesh = new MeshIndirectField();
        public MaterialIndirectField Material = new MaterialIndirectField();
    }

    [Serializable]
    public class EnvironmentTemplatePrefabRuntimeData
    {
        public GameObjectIndirectField Prefab = new GameObjectIndirectField();
    }

    [Serializable]
    public class EnvironmentTemplateCategoryRuntimeData
    {
        public string CategoryName;
        public List<EnvironmentTemplatePrefabRuntimeData> PrefabList = new List<EnvironmentTemplatePrefabRuntimeData>();
        public List<EnvironmentTemplateInstanceMeshRuntimeData> InstanceMeshDataList = new List<EnvironmentTemplateInstanceMeshRuntimeData>();
    }

    public class EnvironmentTemplateEditRuntimeData
    {
        public string Name;

        public List<EnvironmentTemplateCategoryRuntimeData> CategoryDataList = new List<EnvironmentTemplateCategoryRuntimeData>();

        public EnvironmentTemplateEditRuntimeData(EnvironmentTemplateEditData editData)
        {
            Name = editData.name;
        }
    }
}
