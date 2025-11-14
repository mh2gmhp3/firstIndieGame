using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TerrainModule.Editor
{
    public static class TerrainEditorUtility
    {
        public static string[] GetTerrainEditDataNames()
        {
            var editDataGUIDs = AssetDatabase.FindAssets("t:TerrainEditData", new string[] { TerrainEditorDefine.EditTerrainDataFolderPath });
            var result = new string[editDataGUIDs.Length];
            for (int i = 0; i < editDataGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(editDataGUIDs[i]);
                var name = Path.GetFileNameWithoutExtension(assetPath);
                result[i] = name;
            }
            return result;
        }

        public static string[] GetBlockTemplateEditDataNames()
        {
            var editDataGUIDs = AssetDatabase.FindAssets("t:BlockTemplateEditData", new string[] { TerrainEditorDefine.EditBlockTemplateDataFolderPath });
            var result = new string[editDataGUIDs.Length];
            for (int i = 0; i < editDataGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(editDataGUIDs[i]);
                var name = Path.GetFileNameWithoutExtension(assetPath);
                result[i] = name;
            }
            return result;
        }

        /// <summary>
        /// 繪製Chunk網格
        /// </summary>
        /// <param name="blockSize"></param>
        /// <param name="chunkBlockNum"></param>
        /// <param name="chunkNum"></param>
        /// <param name="color"></param>
        /// <param name="startChunkNumY"></param>
        public static void HandleDrawChunk(
            Vector3Int blockSize,
            Vector3Int chunkBlockNum,
            Vector3Int chunkNum,
            Color color,
            int startChunkNumY = 0)
        {
            var chunkSize = blockSize * chunkBlockNum;
            Handles.color = color;
            for (int yIndex = 0; yIndex <= chunkNum.y; yIndex++)
            {
                if (startChunkNumY > yIndex)
                    continue;

                for (int zIndex = 0; zIndex <= chunkNum.z; zIndex++)
                {
                    var start = new Vector3(
                        0,
                        yIndex * chunkSize.y,
                        zIndex * chunkSize.z);
                    var end = new Vector3(
                            chunkNum.x * chunkSize.x,
                            yIndex * chunkSize.y,
                            zIndex * chunkSize.z);
                    Handles.DrawLine(start, end);
                }

                for (int xIndex = 0; xIndex <= chunkNum.x; xIndex++)
                {
                    var start = new Vector3(
                            xIndex * chunkSize.x,
                            yIndex * chunkSize.y,
                            0);
                    var end = new Vector3(
                            xIndex * chunkSize.x,
                            yIndex * chunkSize.y,
                            chunkNum.z * chunkSize.z);
                    Handles.DrawLine(start, end);
                }
            }

            var startPosY = Mathf.Max(0, startChunkNumY) * chunkSize.y;
            for (int zIndex = 0; zIndex <= chunkNum.z; zIndex++)
            {
                for (int xIndex = 0; xIndex <= chunkNum.x; xIndex++)
                {
                    var start = new Vector3(
                            xIndex * chunkSize.x,
                            startPosY,
                            zIndex * chunkSize.z);
                    var end = new Vector3(
                            xIndex * chunkSize.x,
                            chunkNum.y * chunkSize.y,
                            zIndex * chunkSize.z);
                    Handles.DrawLine(start, end);
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

        /// <summary>
        /// 繪製指定ChunkNumY的Plane
        /// </summary>
        /// <param name="blockSize"></param>
        /// <param name="chunkBlockNum"></param>
        /// <param name="chunkNum"></param>
        /// <param name="color"></param>
        /// <param name="chunkNumY"></param>
        public static void HandleDrawChunkPlane(
            Vector3Int blockSize,
            Vector3Int chunkBlockNum,
            Vector3Int chunkNum,
            Color color,
            int chunkNumY)
        {
            var terrainSize = blockSize * chunkBlockNum * chunkNum;
            var y = chunkNumY * blockSize.y * chunkBlockNum.y;
            var verts = new Vector3[]
            {
                new Vector3(0, y, 0),
                new Vector3(terrainSize.x, y, 0),
                new Vector3(terrainSize.x, y, terrainSize.z),
                new Vector3(0, y, terrainSize.z)
            };
            Handles.DrawSolidRectangleWithOutline(verts, color, new Color(0, 0, 0, 0));
        }
    }
}
