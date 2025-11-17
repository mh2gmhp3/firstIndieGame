using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TerrainModule
{
    public static class TerrainDefine
    {
        public const string TerrainDataExportFolderPath = "Assets/Resources/TerrainData";
        public const string TerrainDataResourcesFolderPath = "TerrainData";

        #region Export

        public static string GetExportFolderPath(string dataName)
        {
            return Path.Combine(TerrainDataExportFolderPath, dataName);
        }

        #region TerrainData

        public static string GetExportTerrainDataPath(string dataName)
        {
            return Path.Combine(GetExportFolderPath(dataName), "TerrainData.asset");
        }

        #endregion

        #region Material

        public static string GetExportMaterialFolderPath(string dataName)
        {
            return Path.Combine(GetExportFolderPath(dataName), "Material");
        }

        public static string GetExportMaterialPath(string dataName)
        {
            return Path.Combine(GetExportMaterialFolderPath(dataName), "Terrain.mat");
        }

        #endregion

        #region ChunkMesh

        public static string GetExportChunkMeshFolderPath(string dataName)
        {
            return Path.Combine(GetExportFolderPath(dataName), "ChunkMesh");
        }

        public static string GetExportChunkMeshPath(string dataName, string meshName)
        {
            return Path.Combine(GetExportChunkMeshFolderPath(dataName), meshName + ".mesh");
        }

        #endregion

        #endregion

        #region Resources

        public static string GetResourcesFolderPath(string dataName)
        {
            return Path.Combine(TerrainDataResourcesFolderPath, dataName);
        }

        #region TerrainData

        public static string GetResourcesTerrainDataPath(string dataName)
        {
            return Path.Combine(GetResourcesFolderPath(dataName), "TerrainData");
        }

        #endregion

        #region Material

        public static string GetResourcesMaterialFolderPath(string dataName)
        {
            return Path.Combine(GetResourcesFolderPath(dataName), "Material");
        }

        public static string GetResourcesMaterialPath(string dataName)
        {
            return Path.Combine(GetResourcesMaterialFolderPath(dataName), "Terrain");
        }

        #endregion

        #region ChunkMesh

        public static string GetResourcesChunkMeshFolderPath(string dataName)
        {
            return Path.Combine(GetResourcesFolderPath(dataName), "ChunkMesh");
        }

        public static string GetResourcesChunkMeshPath(string dataName, string meshName)
        {
            return Path.Combine(GetResourcesChunkMeshFolderPath(dataName), meshName);
        }

        #endregion

        #endregion
    }
}
