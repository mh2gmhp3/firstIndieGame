using System.Collections.Generic;
using UnityEngine;
using Utility;
using Extension;
using System;
using static TerrainModule.TerrainDefine;

namespace TerrainModule.Editor
{
    public enum AddInstanceMeshResult
    {
        Success,
        ObjectIsNull,
        CategoryNotFound,
        DuplicateName,
        DoNotHaveAnyMesh,
    }

    public enum AddPrefabResult
    {
        Success,
        ObjectIsNull,
        CategoryNotFound,
        DuplicateName
    }

    public class EnvironmentTemplateColliderEditRuntimeData
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
    }

    public class EnvironmentTemplateInstanceMeshEditRuntimeSingleData
    {
        public MeshIndirectField Mesh = new MeshIndirectField();
        public MaterialIndirectField Material = new MaterialIndirectField();
        public Matrix4x4 Matrix;

        public EnvironmentTemplateInstanceMeshEditRuntimeSingleData(Mesh mesh, Material material, Matrix4x4 matrix)
        {
            Mesh.EditorInstance = mesh;
            Material.EditorInstance = material;
            Matrix = matrix;
        }

        public Bounds GetMeshBounds()
        {
            if (Mesh.EditorInstance == null)
                return new Bounds();

            return Mesh.EditorInstance.bounds;
        }
    }

    public class EnvironmentTemplateInstanceMeshEditRuntimeData
    {
        public GameObjectIndirectField OriginObject = new GameObjectIndirectField();
        public List<EnvironmentTemplateInstanceMeshEditRuntimeSingleData> MeshSingleDataList = new List<EnvironmentTemplateInstanceMeshEditRuntimeSingleData>();
        public List<EnvironmentTemplateColliderEditRuntimeData> ColliderDataList = new List<EnvironmentTemplateColliderEditRuntimeData>();

        public TerrainPreviewInfo PreviewInfo = new TerrainPreviewInfo();

        public EnvironmentTemplateInstanceMeshEditRuntimeData(GameObject originObject)
        {
            OriginObject.EditorInstance = originObject;
            RefreshMeshData();
        }

        public EnvironmentTemplateInstanceMeshEditRuntimeData(EnvironmentTemplateInstanceMeshEditData editData)
        {
            editData.OriginObject.CopyTo(OriginObject);
            RefreshMeshData();
        }

        private void RefreshMeshData()
        {
            MeshSingleDataList.Clear();
            var meshDataList = TerrainEditorUtility.GetGameObjectMeshData(OriginObject.EditorInstance);
            for (int i = 0; i < meshDataList.Count; i++)
            {
                var meshData = meshDataList[i];
                MeshSingleDataList.Add(new EnvironmentTemplateInstanceMeshEditRuntimeSingleData(meshData.Mesh, meshData.Material, meshData.Matrix));
            }
            var colliderDataList = TerrainEditorUtility.GetGameObjectColliderData(OriginObject.EditorInstance);
            for (int i = 0; i < colliderDataList.Count; i++)
            {
                var colliderData = colliderDataList[i];
                EnvironmentTemplateColliderEditRuntimeData colliderEditData = null;
                if (colliderData.ColliderType == ColliderType.Box)
                {
                    colliderEditData = new EnvironmentTemplateColliderEditRuntimeData()
                    {
                        ColliderType = ColliderType.Box,
                        Center = colliderData.Center,
                        Size = colliderData.Size,
                        Position = colliderData.Position,
                        Rotation = colliderData.Rotation,
                        Scale = colliderData.Scale,
                    };                }
                else if (colliderData.ColliderType == ColliderType.Sphere)
                {
                    colliderEditData = new EnvironmentTemplateColliderEditRuntimeData()
                    {
                        ColliderType = ColliderType.Sphere,
                        Center = colliderData.Center,
                        Radius = colliderData.Radius,
                        Position = colliderData.Position,
                        Rotation = colliderData.Rotation,
                        Scale = colliderData.Scale,
                    };
                }
                else if (colliderData.ColliderType == ColliderType.Capsule)
                {
                    colliderEditData = new EnvironmentTemplateColliderEditRuntimeData()
                    {
                        ColliderType = ColliderType.Capsule,
                        Center = colliderData.Center,
                        Radius = colliderData.Radius,
                        Height = colliderData.Height,
                        Direction = colliderData.Direction,
                        Position = colliderData.Position,
                        Rotation = colliderData.Rotation,
                        Scale = colliderData.Scale,
                    };
                }
                else
                {
                    continue;
                }
                ColliderDataList.Add(colliderEditData);
            }
        }
    }

    public class EnvironmentTemplatePrefabEditRuntimeData
    {
        public GameObjectIndirectField Prefab = new GameObjectIndirectField();

        public List<EnvironmentTemplateInstanceMeshEditRuntimeSingleData> TempMeshSingleDataList = new List<EnvironmentTemplateInstanceMeshEditRuntimeSingleData>();

        public TerrainPreviewInfo PreviewInfo = new TerrainPreviewInfo();

        public EnvironmentTemplatePrefabEditRuntimeData(GameObject go)
        {
            Prefab.EditorInstance = go;
            RefreshMeshData();
        }

        public EnvironmentTemplatePrefabEditRuntimeData(EnvironmentTemplatePrefabEditData editData)
        {
            editData.Prefab.CopyTo(Prefab);
            RefreshMeshData();
        }

        private void RefreshMeshData()
        {
            TempMeshSingleDataList.Clear();
            var meshDataList = TerrainEditorUtility.GetGameObjectMeshData(Prefab.EditorInstance);
            for (int i = 0; i < meshDataList.Count; i++)
            {
                var meshData = meshDataList[i];
                TempMeshSingleDataList.Add(new EnvironmentTemplateInstanceMeshEditRuntimeSingleData(meshData.Mesh, meshData.Material, meshData.Matrix));
            }
        }
    }

    public class EnvironmentTemplateCategoryEditRuntimeData
    {
        public string CategoryName;
        public List<EnvironmentTemplatePrefabEditRuntimeData> PrefabList = new List<EnvironmentTemplatePrefabEditRuntimeData>();
        public List<EnvironmentTemplateInstanceMeshEditRuntimeData> InstanceMeshDataList = new List<EnvironmentTemplateInstanceMeshEditRuntimeData>();

        public EnvironmentTemplateCategoryEditRuntimeData(string name)
        {
            CategoryName = name;
        }

        public EnvironmentTemplateCategoryEditRuntimeData(EnvironmentTemplateCategoryEditData editData)
        {
            CategoryName = editData.CategoryName;
            for (int i = 0; i < editData.PrefabList.Count; i++)
            {
                PrefabList.Add(new EnvironmentTemplatePrefabEditRuntimeData(editData.PrefabList[i]));
            }
            for (int i = 0; i < editData.InstanceMeshDataList.Count; i++)
            {
                InstanceMeshDataList.Add(new EnvironmentTemplateInstanceMeshEditRuntimeData(editData.InstanceMeshDataList[i]));
            }
        }

        public bool IsExistInstanceMeshName(string name)
        {
            for (int i = 0; i < InstanceMeshDataList.Count; i++)
            {
                var go = InstanceMeshDataList[i].OriginObject.EditorInstance;
                if (go == null)
                    continue;

                if (go.name == name)
                    return true;
            }
            return false;
        }

        public bool IsExistPrefabName(string name)
        {
            for (int i = 0; i < PrefabList.Count; i++)
            {
                var go = PrefabList[i].Prefab.EditorInstance;
                if (go == null)
                    continue;

                if (go.name == name)
                    return true;
            }
            return false;
        }

        public bool TryGetPrefab(string name, out EnvironmentTemplatePrefabEditRuntimeData prefabData)
        {
            prefabData = null;
            for (int i = 0; i < PrefabList.Count; i++)
            {
                var go = PrefabList[i].Prefab.EditorInstance;
                if (go == null)
                    continue;

                if (go.name == name)
                {
                    prefabData = PrefabList[i];
                    return true;
                }
            }
            return false;
        }

        public bool TryGetInsMesh(string name, out EnvironmentTemplateInstanceMeshEditRuntimeData insMeshData)
        {
            insMeshData = null;
            for (int i = 0; i < InstanceMeshDataList.Count; i++)
            {
                var go = InstanceMeshDataList[i].OriginObject.EditorInstance;
                if (go == null)
                    continue;

                if (go.name == name)
                {
                    insMeshData = InstanceMeshDataList[i];
                    return true;
                }
            }
            return false;
        }

        public bool TryGetName(bool isInsMesh, int index, out string name)
        {
            name = string.Empty;
            if (isInsMesh)
            {
                if (!InstanceMeshDataList.TryGet(index, out var insMeshData))
                    return false;
                var go = insMeshData.OriginObject.EditorInstance;
                if (go == null)
                    return false;
                name = go.name;
                return true;
            }
            else
            {
                if (!PrefabList.TryGet(index, out var insMeshData))
                    return false;
                var go = insMeshData.Prefab.EditorInstance;
                if (go == null)
                    return false;
                name = go.name;
                return true;
            }
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

        public List<EnvironmentTemplateCategoryEditRuntimeData> CategoryDataList = new List<EnvironmentTemplateCategoryEditRuntimeData>();

        public EnvironmentTemplateEditRuntimeData(EnvironmentTemplateEditData editData)
        {
            Name = editData.name;
            for (int i = 0; i < editData.CategoryDataList.Count; i++)
            {
                CategoryDataList.Add(new EnvironmentTemplateCategoryEditRuntimeData(editData.CategoryDataList[i]));
            }
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

            CategoryDataList.Add(new EnvironmentTemplateCategoryEditRuntimeData(name));
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

        public bool TryGetCategory(string name, out EnvironmentTemplateCategoryEditRuntimeData result)
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

        public bool TryGetPrefab(string categoryName, string name, out EnvironmentTemplatePrefabEditRuntimeData prefabData)
        {
            prefabData = null;
            if (!TryGetCategory(categoryName, out var categoryData))
                return false;

            return categoryData.TryGetPrefab(name, out prefabData);
        }

        public bool TryGetInsMesh(string categoryName, string name, out EnvironmentTemplateInstanceMeshEditRuntimeData insMeshData)
        {
            insMeshData = null;
            if (!TryGetCategory(categoryName, out var categoryData))
                return false;

            return categoryData.TryGetInsMesh(name, out insMeshData);
        }

        public AddInstanceMeshResult AddInstanceMesh(string categoryName, GameObject go)
        {
            if (go == null)
                return AddInstanceMeshResult.ObjectIsNull;

            if (!TryGetCategory(categoryName, out var categoryRuntimeData))
                return AddInstanceMeshResult.CategoryNotFound;

            var name = go.name;
            if (categoryRuntimeData.IsExistInstanceMeshName(name))
                return AddInstanceMeshResult.DuplicateName;

            var meshData = new EnvironmentTemplateInstanceMeshEditRuntimeData(go);
            if (meshData.MeshSingleDataList.Count == 0)
                return AddInstanceMeshResult.DoNotHaveAnyMesh;

            categoryRuntimeData.InstanceMeshDataList.Add(meshData);
            return AddInstanceMeshResult.Success;
        }

        public AddPrefabResult AddPrefab(string categoryName, GameObject go)
        {
            if (go == null)
                return AddPrefabResult.ObjectIsNull;

            if (!TryGetCategory(categoryName, out var categoryRuntimeData))
                return AddPrefabResult.CategoryNotFound;

            var name = go.name;
            if (categoryRuntimeData.IsExistPrefabName(name))
                return AddPrefabResult.DuplicateName;

            var prefabData = new EnvironmentTemplatePrefabEditRuntimeData(go);
            categoryRuntimeData.PrefabList.Add(prefabData);
            return AddPrefabResult.Success;
        }
    }
}
