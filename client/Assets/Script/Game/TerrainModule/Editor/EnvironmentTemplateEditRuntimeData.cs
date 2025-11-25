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

        public EnvironmentTemplateCategoryRuntimeData(string name)
        {
            CategoryName = name;
        }
    }

    public class EnvironmentTemplateEditRuntimeData
    {
        public enum AddCategoryResult
        {
            Success,
            NameIsEmpty,
            NameDuplicate
        }

        public string Name;

        public List<EnvironmentTemplateCategoryRuntimeData> CategoryDataList = new List<EnvironmentTemplateCategoryRuntimeData>();

        public EnvironmentTemplateEditRuntimeData(EnvironmentTemplateEditData editData)
        {
            Name = editData.name;
        }

        public string[] GetCategoryNames()
        {
            var result = new string[CategoryDataList.Count];
            for (int i = 0; i < CategoryDataList.Count; i++)
            {
                result[i] = CategoryDataList[i].CategoryName;
            }
            return result;
        }

        public AddCategoryResult AddCategory(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return AddCategoryResult.NameIsEmpty;
            }

            for (int i = 0; i < CategoryDataList.Count; i++)
            {
                if (CategoryDataList[i].CategoryName == name)
                {
                    return AddCategoryResult.NameDuplicate;
                }
            }

            CategoryDataList.Add(new EnvironmentTemplateCategoryRuntimeData(name));
            return AddCategoryResult.Success;
        }

        public bool RemoveCategory(string name)
        {
            for (int i = 0; i < CategoryDataList.Count; i++)
            {
                if (CategoryDataList[i].CategoryName == name)
                {
                    CategoryDataList.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public bool TryGetCategory(string name, out EnvironmentTemplateCategoryRuntimeData result)
        {
            for (int i = 0; i < CategoryDataList.Count; i++)
            {
                if (CategoryDataList[i].CategoryName == name)
                {
                    result = CategoryDataList[i];
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}
