using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TerrainModule.Editor
{
    public static class TerrainEditorUtility
    {
        public static string[] GetEditDataFolderNames()
        {
            var editDataGUIDs = AssetDatabase.FindAssets("t:TerrainEditData", new string[] { TerrainEditorDefine.EditDataFolderPath });
            var result = new string[editDataGUIDs.Length];
            for (int i = 0; i < editDataGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(editDataGUIDs[i]);
                var name = Path.GetFileNameWithoutExtension(assetPath);
                result[i] = name;
            }
            return result;
        }

        public static void HandleDrawChunk(Vector3Int blockSize, Vector3Int chunkBlockNum, Vector3Int chunkNum)
        {
            var chunkSize = blockSize * chunkBlockNum;
            var centerRedress = new Vector3(
                chunkSize.x / 2f,
                chunkSize.y / 2f,
                chunkSize.z / 2f);
            for (int yIndex = 0; yIndex < chunkNum.y; yIndex++)
            {
                for (int zIndex = 0; zIndex < chunkNum.z; zIndex++)
                {
                    for (int xIndex = 0; xIndex < chunkNum.x; xIndex++)
                    {
                        var center = new Vector3(
                                xIndex * chunkSize.x,
                                yIndex * chunkSize.y,
                                zIndex * chunkSize.z) + centerRedress;
                        Handles.DrawWireCube(center, chunkSize);
                    }
                }
            }
            //Handles.BeginGUI();
            //for (int yIndex = 0; yIndex < chunkNum.y; yIndex++)
            //{
            //    for (int zIndex = 0; zIndex < chunkNum.z; zIndex++)
            //    {
            //        for (int xIndex = 0; xIndex < chunkNum.x; xIndex++)
            //        {
            //            var worldPos = new Vector3(
            //                    xIndex * chunkSize.x,
            //                    yIndex * chunkSize.y,
            //                    zIndex * chunkSize.z);
            //            var rectPos = HandleUtility.WorldToGUIPoint(worldPos);
            //            GUI.Label(new Rect(rectPos, new Vector2(30, 30)), worldPos.ToString());
            //        }
            //    }
            //}
            //Handles.EndGUI();
        }
    }
}
