using Framework.Editor;
using System;
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

        public static Material GenTerrainMaterial(Shader shader, Texture2D tileMap, Vector2 tiling)
        {
            var result = new Material(shader);
            result.SetTexture("_TileMap", tileMap);
            result.SetVector("_Tiling", new Vector4(tiling.x, tiling.y, 0, 0));
            return result;
        }

        public static (Vector4 TopValue, Vector4 BottomValue) ClampYValue(Vector4 topValue, Vector4 bottomValue)
        {
            topValue = new Vector4(
                Mathf.Clamp01(topValue.x),
                Mathf.Clamp01(topValue.y),
                Mathf.Clamp01(topValue.z),
                Mathf.Clamp01(topValue.w));
            bottomValue = new Vector4(
                Mathf.Clamp01(bottomValue.x),
                Mathf.Clamp01(bottomValue.y),
                Mathf.Clamp01(bottomValue.z),
                Mathf.Clamp01(bottomValue.w));
            topValue = new Vector4(
                (float)Math.Round((double)Mathf.Clamp(topValue.x, bottomValue.x, 1), 1),
                (float)Math.Round((double)Mathf.Clamp(topValue.y, bottomValue.y, 1), 1),
                (float)Math.Round((double)Mathf.Clamp(topValue.z, bottomValue.z, 1), 1),
                (float)Math.Round((double)Mathf.Clamp(topValue.w, bottomValue.w, 1), 1));
            bottomValue = new Vector4(
                (float)Math.Round((double)Mathf.Clamp(bottomValue.x, 0, topValue.x), 1),
                (float)Math.Round((double)Mathf.Clamp(bottomValue.y, 0, topValue.y), 1),
                (float)Math.Round((double)Mathf.Clamp(bottomValue.z, 0, topValue.z), 1),
                (float)Math.Round((double)Mathf.Clamp(bottomValue.w, 0, topValue.w), 1));
            return (topValue, bottomValue);
        }

        public static void DrawTiling(BlockTemplateRuntimeData data)
        {
            EditorGUILayout.BeginVertical();
            {
                DrawTiling("+x", data.PXTiling, "-x", data.NXTiling);
                DrawTiling("+y", data.PYTiling, "-y", data.NYTiling);
                DrawTiling("+z", data.PZTiling, "-z", data.NZTiling);
            }
            EditorGUILayout.EndVertical();
        }

        private static void DrawTiling(string pLabel, Vector2 pTiling, string nLabel, Vector2 nTiling)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField($"{pLabel}:{pTiling}");
                EditorGUILayout.LabelField($"{nLabel}:{nTiling}");
            }
            EditorGUILayout.EndHorizontal();
        }

        public static Vector3Int DrawSelectableTiling(BlockTemplateRuntimeData data, Vector3Int selectFace)
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                selectFace = DrawSelectableTiling("+x", data.PXTiling, Vector3Int.right, "-x", data.NXTiling, Vector3Int.left, selectFace);
                GUILayout.Space(2f);
                selectFace = DrawSelectableTiling("+y", data.PYTiling, Vector3Int.up, "-y", data.NYTiling, Vector3Int.down, selectFace);
                GUILayout.Space(2f);
                selectFace = DrawSelectableTiling("+z", data.PZTiling, Vector3Int.forward, "-z", data.NZTiling, Vector3Int.back, selectFace);
            }
            EditorGUILayout.EndVertical();

            return selectFace;
        }

        private static Vector3Int DrawSelectableTiling(string pLabel, Vector2 pTiling, Vector3Int pFace, string nLabel, Vector2 nTiling, Vector3Int nFace, Vector3Int selectFace)
        {
            var pTilingRect = new Rect();
            var nTilingRect = new Rect();
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.SelectableBlueBox(pFace == selectFace));
                {
                    EditorGUILayout.LabelField($"{pLabel}");
                    EditorGUILayout.Vector2Field("", pTiling);
                }
                EditorGUILayout.EndHorizontal();
                pTilingRect = GUILayoutUtility.GetLastRect();
                GUILayout.Space(2f);
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.SelectableBlueBox(nFace == selectFace));
                {
                    EditorGUILayout.LabelField($"{nLabel}");
                    EditorGUILayout.Vector2Field("", nTiling);
                }
                EditorGUILayout.EndHorizontal();
                nTilingRect = GUILayoutUtility.GetLastRect();
            }
            EditorGUILayout.EndHorizontal();
            var current = Event.current;
            if (current.isMouse)
            {
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    if (pTilingRect.Contains(current.mousePosition))
                    {
                        GUI.changed = true;
                        return pFace;
                    }
                    else if (nTilingRect.Contains(current.mousePosition))
                    {
                        GUI.changed = true;
                        return nFace;
                    }
                }
            }

            return selectFace;
        }

        /// <summary>
        /// 繪製方塊範本預覽圖
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mgr"></param>
        /// <param name="material"></param>
        /// <param name="blockSize"></param>
        /// <param name="rect"></param>
        /// <param name="backgroundColor"></param>
        public static void DrawBlockTemplatePreview(
            BlockTemplateRuntimeData data,
            TerrainEditorManager mgr,
            Material material,
            Vector3Int blockSize,
            Rect rect,
            Color backgroundColor)
        {
            if (rect.x == 0 && rect.y == 0 && rect.width == 1 && rect.height == 1)
                return;

            var previewInfo = data.PreviewInfo;

            Event current = Event.current;
            if (rect.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseDrag && current.button == 0)
                {
                    previewInfo.Rotation.x += current.delta.x;
                    previewInfo.Rotation.y -= current.delta.y;

                    previewInfo.Rotation.y = Mathf.Clamp(previewInfo.Rotation.y, -90f, 90f);
                    previewInfo.MarkRefreshTexture();
                    current.Use(); // 標記事件已處理
                }
                if (current.type == EventType.ScrollWheel)
                {
                    var distance = Mathf.Max(Mathf.Max(blockSize.x, blockSize.y), blockSize.z);
                    previewInfo.Distance += current.delta.y * 0.1f;
                    previewInfo.Distance = Mathf.Clamp(previewInfo.Distance, distance, distance * 2f);
                    previewInfo.MarkRefreshTexture();
                    current.Use(); // 標記事件已處理
                }
            }

            if (previewInfo.Texture == null)
            {
                if (previewInfo.Distance == 0)
                {
                    var distance = Mathf.Max(Mathf.Max(blockSize.x, blockSize.y), blockSize.z);
                    previewInfo.Distance = distance * 2;
                }
                Quaternion camRotation = Quaternion.Euler(-previewInfo.Rotation.y, previewInfo.Rotation.x, 0);
                Vector3 camPos = camRotation * Vector3.back * previewInfo.Distance;

                var rt = mgr.GetBlockPreviewTexture(data, material, new Vector2(300f, 300f), blockSize, camPos, camRotation);
                if (previewInfo.CachedTexture == null)
                    previewInfo.CachedTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                RenderTexture.active = (RenderTexture)rt;
                previewInfo.CachedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                previewInfo.CachedTexture.Apply();
                RenderTexture.active = null;

                previewInfo.Texture = previewInfo.CachedTexture;
            }

            EditorGUI.DrawRect(rect, Color.black);
            EditorGUI.DrawRect(rect, backgroundColor);
            GUI.DrawTexture(rect, previewInfo.Texture);
        }

        /// <summary>
        /// 繪製方塊範本預覽圖
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mgr"></param>
        /// <param name="blockSize"></param>
        /// <param name="rect"></param>
        public static void DrawBlockTemplatePreview(
            BlockTemplateRuntimeData data,
            TerrainEditorManager mgr,
            Material material,
            Vector3Int blockSize,
            Rect rect)
        {
            DrawBlockTemplatePreview(data, mgr, material, blockSize, rect, new Color(0, 0, 1, 0.5f));
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
