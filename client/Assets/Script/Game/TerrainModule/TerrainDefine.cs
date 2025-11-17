using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TerrainModule
{
    public static class TerrainDefine
    {
        public const string TerrainDataExportFolderPath = "Assets/Resources/TerrainData";

        public static string GetExportFolderPath(string dataName)
        {
            return Path.Combine(TerrainDataExportFolderPath, dataName);
        }

        public static string GetExportMaterialFolderPath(string dataName)
        {
            return Path.Combine(GetExportFolderPath(dataName), "Material");
        }

        public static string GetExportChunkMeshFolderPath(string dataName)
        {
            return Path.Combine(GetExportFolderPath(dataName), "ChunkMesh");
        }
    }
}
