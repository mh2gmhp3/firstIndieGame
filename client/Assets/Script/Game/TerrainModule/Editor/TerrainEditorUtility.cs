using Framework.Editor;
using Framework.Editor.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Extension;
using static TerrainModule.Editor.TerrainEditorManager;

namespace TerrainModule.Editor
{
    public class BlockTemplatePreviewSetting
    {
        public Vector3Int BlockSize = Vector3Int.one;
        public Vector4 YTopValue = Vector4.one;
        public Vector4 YBottomValue = Vector4.zero;

        public float PXRotation;
        public float NXRotation;

        public float PYRotation;
        public float NYRotation;

        public float PZRotation;
        public float NZRotation;

        public int PreviewId;

        public void SetYValue(Vector4 topValue, Vector4 bottomValue)
        {
            var clampValue = TerrainEditorUtility.ClampYValue(topValue, bottomValue);
            YTopValue = clampValue.TopValue;
            YBottomValue = clampValue.BottomValue;
        }
    }

    public static class TerrainEditorUtility
    {
        #region GetEditDataNames

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

        #endregion

        #region Material

        public static Material GenTerrainMaterial(Shader shader, Texture2D tileMap, Vector2 tiling)
        {
            var result = new Material(shader);
            result.SetTexture("_TileMap", tileMap);
            result.SetVector("_Tiling", new Vector4(tiling.x, tiling.y, 0, 0));
            return result;
        }

        #endregion

        #region YValue

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

        #endregion

        #region Tiling

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

        #endregion

        #region BlockPreviewGUI

        /// <summary>
        /// 繪製方塊範本預覽圖
        /// </summary>
        /// <param name="data"></param>
        /// <param name="previewSetting"></param>
        /// <param name="material"></param>
        /// <param name="rect"></param>
        /// <param name="backgroundColor"></param>
        public static void DrawBlockTemplatePreview(
            BlockTemplateRuntimeData data,
            BlockTemplatePreviewSetting previewSetting,
            Material material,
            Rect rect,
            Color backgroundColor)
        {
            if (rect.x == 0 && rect.y == 0 && rect.width == 1 && rect.height == 1)
                return;

            var previewInfo = data.PreviewInfo;
            var blockSize = previewSetting.BlockSize;

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

                var rt = GetBlockPreviewTexture(data, previewSetting, material, new Vector2(300f, 300f), camPos, camRotation);
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
        /// <param name="previewSetting"></param>
        /// <param name="material"></param>
        /// <param name="rect"></param>
        public static void DrawBlockTemplatePreview(
            BlockTemplateRuntimeData data,
            BlockTemplatePreviewSetting previewSetting,
            Material material,
            Rect rect)
        {
            DrawBlockTemplatePreview(data, previewSetting, material, rect, new Color(0, 0, 1, 0.5f));
        }

        #endregion

        #region Handles

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

        #endregion

        #region Mesh

        public static void CreateChunkMesh(
            Mesh mesh,
            TerrainEditRuntimeData terrainEditData,
            int chunkId)
        {
            if (mesh == null || terrainEditData == null)
                return;

            if (!terrainEditData.IdToChunkEditData.TryGetValue(chunkId, out var chunkEditData))
                return;

            mesh.Clear();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var uvs2 = new List<Vector2>();
            var uvs3 = new List<Vector2>();
            foreach (var blockEditDataPair in chunkEditData.IdToBlockEditData)
            {
                var blockEditData = blockEditDataPair.Value;
                CreateBlock(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    terrainEditData,
                    chunkEditData,
                    blockEditData);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.uv2 = uvs2.ToArray();
            mesh.uv3 = uvs3.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
        }

        public static Mesh CreateChunkMesh(
            TerrainEditRuntimeData terrainEditData,
            int chunkId)
        {
            var mesh = new Mesh();
            CreateChunkMesh(mesh, terrainEditData, chunkId);
            return mesh;
        }

        public static void CreateBlock(
            List<Vector3> vertices,
            List<int> triangles,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Vector2> uvs2,
            List<Vector2> uvs3,
            TerrainEditRuntimeData terrainEditRuntimeData,
            ChunkEditRuntimeData chunkEditRuntimeData,
            BlockEditRuntimeData blockEditRuntimeData)
        {
            var chunkId = chunkEditRuntimeData.Id;
            var blockId = blockEditRuntimeData.Id;
            var BlockTemplateId = blockEditRuntimeData.TemplateId;
            var size = terrainEditRuntimeData.BlockSize;
            var worldBlockCoord = terrainEditRuntimeData.GetWorldBlockCoordWithId(chunkId, blockId);
            var worldBlockPivotPos = worldBlockCoord * size;

            BlockEditRuntimeData refBlockData = null;

            bool createFace = false;
            Vector4 yTopValue = blockEditRuntimeData.YTopValue;
            Vector4 yBottomValue = blockEditRuntimeData.YBottomValue;

            if (!terrainEditRuntimeData.BlockTemplateEditRuntimeData.TryGetBlockData(BlockTemplateId, out var blockTemplateData))
            {
                blockTemplateData = new BlockTemplateRuntimeData(0);
                Debug.Log($"ChunkId:{chunkId} BlockId:{blockId} BlockTemplateId:{BlockTemplateId} 找不到方塊範本, 使用預設");
            }

            //方向沒方塊必須建立面
            //有方塊必須完全共面才不需建立 不考慮交錯要補面問題
            // +x
            var worldRefBlockCoord = worldBlockCoord + Vector3Int.right;
            if (terrainEditRuntimeData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = !(blockEditRuntimeData.YTopValue.y.ApproximateEqual(refBlockData.YTopValue.x)
                    && blockEditRuntimeData.YBottomValue.y.ApproximateEqual(refBlockData.YBottomValue.x)
                    && blockEditRuntimeData.YTopValue.w.ApproximateEqual(refBlockData.YTopValue.z)
                    && blockEditRuntimeData.YBottomValue.w.ApproximateEqual(refBlockData.YBottomValue.z));
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.right,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.PXTiling,
                    blockEditRuntimeData.PXRotation);
            }

            // -x
            worldRefBlockCoord = worldBlockCoord + Vector3Int.left;
            if (terrainEditRuntimeData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = !(blockEditRuntimeData.YTopValue.x.ApproximateEqual(refBlockData.YTopValue.y)
                    && blockEditRuntimeData.YBottomValue.x.ApproximateEqual(refBlockData.YBottomValue.y)
                    && blockEditRuntimeData.YTopValue.z.ApproximateEqual(refBlockData.YTopValue.w)
                    && blockEditRuntimeData.YBottomValue.z.ApproximateEqual(refBlockData.YBottomValue.w));
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.left,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.NXTiling,
                    blockEditRuntimeData.NXRotation);
            }

            // +z
            worldRefBlockCoord = worldBlockCoord + Vector3Int.forward;
            if (terrainEditRuntimeData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = !(blockEditRuntimeData.YTopValue.z.ApproximateEqual(refBlockData.YTopValue.x)
                    && blockEditRuntimeData.YBottomValue.z.ApproximateEqual(refBlockData.YBottomValue.x)
                    && blockEditRuntimeData.YTopValue.w.ApproximateEqual(refBlockData.YTopValue.y)
                    && blockEditRuntimeData.YBottomValue.w.ApproximateEqual(refBlockData.YBottomValue.y));
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.forward,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.PZTiling,
                    blockEditRuntimeData.PZRotation);
            }

            // -z
            worldRefBlockCoord = worldBlockCoord + Vector3Int.back;
            if (terrainEditRuntimeData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = !(blockEditRuntimeData.YTopValue.x.ApproximateEqual(refBlockData.YTopValue.z)
                    && blockEditRuntimeData.YBottomValue.x.ApproximateEqual(refBlockData.YBottomValue.z)
                    && blockEditRuntimeData.YTopValue.y.ApproximateEqual(refBlockData.YTopValue.w)
                    && blockEditRuntimeData.YBottomValue.y.ApproximateEqual(refBlockData.YBottomValue.w));
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.back,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.NZTiling,
                    blockEditRuntimeData.NZRotation);
            }

            // +y
            worldRefBlockCoord = worldBlockCoord + Vector3Int.up;
            if (terrainEditRuntimeData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = blockEditRuntimeData.YTopValue != Vector4.one || refBlockData.YBottomValue != Vector4.zero;
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.up,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.PYTiling,
                    blockEditRuntimeData.PYRotation);
            }

            // -y
            worldRefBlockCoord = worldBlockCoord + Vector3Int.down;
            if (terrainEditRuntimeData.TryGetBlock(worldRefBlockCoord, out refBlockData, out _))
            {
                createFace = blockEditRuntimeData.YBottomValue != Vector4.zero || refBlockData.YTopValue != Vector4.one;
            }
            else
            {
                createFace = true;
            }
            if (createFace)
            {
                CreateBlockFace(
                    vertices,
                    triangles,
                    normals,
                    uvs,
                    uvs2,
                    uvs3,
                    worldBlockPivotPos,
                    size,
                    Vector3Int.down,
                    yTopValue,
                    yBottomValue,
                    blockTemplateData.NYTiling,
                    blockEditRuntimeData.NYRotation);
            }
        }

        public static void CreatePreviewBlockMesh(
            Mesh mesh,
            BlockTemplateRuntimeData blockTemplate,
            BlockTemplatePreviewSetting previewSetting,
            Vector3 worldBlockPivotPos)
        {
            var size = previewSetting.BlockSize;
            Vector4 yTopValue = previewSetting.YTopValue;
            Vector4 yBottomValue = previewSetting.YBottomValue;

            mesh.Clear();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var uvs2 = new List<Vector2>();
            var uvs3 = new List<Vector2>();

            // +x
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.right,
                yTopValue,
                yBottomValue,
                blockTemplate.PXTiling,
                previewSetting.PXRotation);

            // -x
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.left,
                yTopValue,
                yBottomValue,
                blockTemplate.NXTiling,
                previewSetting.NXRotation);

            // +z
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.forward,
                yTopValue,
                yBottomValue,
                blockTemplate.PZTiling,
                previewSetting.PZRotation);

            // -z
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.back,
                yTopValue,
                yBottomValue,
                blockTemplate.NZTiling,
                previewSetting.NZRotation);

            // +y
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.up,
                yTopValue,
                yBottomValue,
                blockTemplate.PYTiling,
                previewSetting.PYRotation);

            // -y
            CreateBlockFace(
                vertices,
                triangles,
                normals,
                uvs,
                uvs2,
                uvs3,
                worldBlockPivotPos,
                size,
                Vector3Int.down,
                yTopValue,
                yBottomValue,
                blockTemplate.NYTiling,
                previewSetting.NYRotation);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.uv2 = uvs2.ToArray();
            mesh.uv3 = uvs3.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
        }

        #region Face

        private static void CreateBlockFace(
            List<Vector3> vertices,
            List<int> triangles,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Vector2> uvs2,
            List<Vector2> uvs3,
            Vector3 worldPosition,
            Vector3 size,
            Vector3Int faceNormal,
            Vector4 yTopValue,
            Vector4 yBottomValue,
            Vector2 tiling,
            float rotationValue)
        {
            Vector4 topY = size.y * yTopValue;
            Vector4 bottomY = size.y * yBottomValue;

            Vector3[] addVertices = null;
            int[] addTriangles = null;
            Vector2[] addUVs = null;

            //+x
            if (faceNormal == Vector3Int.right)
            {
                var p0Pos = worldPosition + new Vector3(size.x, 0, 0);
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.y, 0),                                   // Bottom-left
                    p0Pos + new Vector3(0, bottomY.w, size.z),                              // Bottom-right
                    p0Pos + new Vector3(0, topY.y, 0),                                      // Top-left
                    p0Pos + new Vector3(0, topY.w, size.z)                                  // Top-right
                };
                addTriangles = GetTriangle(vertices.Count, false);
                addUVs = new Vector2[]
                {
                    new Vector2(0, yBottomValue.y),
                    new Vector2(1 , yBottomValue.w),
                    new Vector2(0, yTopValue.y),
                    new Vector2(1, yTopValue.w)
                };
            }
            //-x
            else if (faceNormal == Vector3Int.left)
            {
                var p0Pos = worldPosition + new Vector3(0, 0, size.z);
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.z, 0),                                    // Bottom-left
                    p0Pos + new Vector3(0, bottomY.x, -size.z),                              // Bottom-right
                    p0Pos + new Vector3(0, topY.z, 0),                                       // Top-left
                    p0Pos + new Vector3(0, topY.x, -size.z)                                  // Top-right
                };
                addTriangles = GetTriangle(vertices.Count, false);
                addUVs = new Vector2[]
                {
                    new Vector2(0, yBottomValue.z),
                    new Vector2(1 , yBottomValue.x),
                    new Vector2(0, yTopValue.z),
                    new Vector2(1, yTopValue.x)
                };
            }
            //+z
            else if (faceNormal == Vector3Int.forward)
            {
                var p0Pos = worldPosition + new Vector3(size.x, 0, size.z);
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.w, 0),                                    // Bottom-left
                    p0Pos + new Vector3(-size.x, bottomY.z, 0),                              // Bottom-right
                    p0Pos + new Vector3(0, topY.w, 0) ,                                      // Top-left
                    p0Pos + new Vector3(-size.x, topY.z, 0)                                  // Top-right
                };
                addTriangles = GetTriangle(vertices.Count, false);
                addUVs = new Vector2[]
                {
                    new Vector2(0, yBottomValue.w),
                    new Vector2(1 , yBottomValue.z),
                    new Vector2(0, yTopValue.w),
                    new Vector2(1, yTopValue.z)
                };
            }
            //-z
            else if (faceNormal == Vector3Int.back)
            {
                var p0Pos = worldPosition;
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.x, 0),                                    // Bottom-left
                    p0Pos + new Vector3(size.x, bottomY.y, 0),                               // Bottom-right
                    p0Pos + new Vector3(0, topY.x, 0) ,                                      // Top-left
                    p0Pos + new Vector3(size.x, topY.y, 0)                                   // Top-right
                };
                addTriangles = GetTriangle(vertices.Count, false);
                addUVs = new Vector2[]
                {
                    new Vector2(0, yBottomValue.x),
                    new Vector2(1 , yBottomValue.y),
                    new Vector2(0, yTopValue.x),
                    new Vector2(1, yTopValue.y)
                };
            }
            //+y
            else if (faceNormal == Vector3Int.up)
            {
                var p0Pos = worldPosition;
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, topY.x, 0),                                    // Bottom-left
                    p0Pos + new Vector3(size.x, topY.y, 0),                               // Bottom-right
                    p0Pos + new Vector3(0, topY.z, size.z) ,                              // Top-left
                    p0Pos + new Vector3(size.x, topY.w, size.z)                           // Top-right
                };
                bool mirrorTriangle =
                    yTopValue.x > yTopValue.y ||
                    yTopValue.w > yTopValue.z;
                addTriangles = GetTriangle(vertices.Count, mirrorTriangle);
                addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
            }
            //-y
            else if (faceNormal == Vector3Int.down)
            {
                var p0Pos = worldPosition + new Vector3(0, 0, size.z);
                addVertices = new Vector3[]
                {
                    p0Pos + new Vector3(0, bottomY.z, 0),                                     // Bottom-left
                    p0Pos + new Vector3(size.x, bottomY.w, 0),         // Bottom-right
                    p0Pos + new Vector3(0, bottomY.x, -size.z) ,       // Top-left
                    p0Pos + new Vector3(size.x, bottomY.y, -size.z)    // Top-right
                };
                bool mirrorTriangle =
                    yBottomValue.x > yBottomValue.y ||
                    yBottomValue.w > yBottomValue.z;
                addTriangles = GetTriangle(vertices.Count, mirrorTriangle);
                addUVs = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
            }
            else
            {
                return;
            }

            var addUVs2 = new Vector2[]
            {
                tiling,
                tiling,
                tiling,
                tiling
            };
            var rotation = new Vector2(rotationValue, 0);
            var addUVs3 = new Vector2[]
            {
                rotation,
                rotation,
                rotation,
                rotation
            };

            vertices.AddRange(addVertices);
            triangles.AddRange(addTriangles);
            uvs.AddRange(addUVs);
            uvs2.AddRange(addUVs2);
            uvs3.AddRange(addUVs3);
        }

        private static int[] GetTriangle(int startIndex, bool isMirror)
        {
            if (!isMirror)
            {
                return new int[]
                {
                    startIndex + 0, startIndex + 2, startIndex + 1,
                    startIndex + 2, startIndex + 3, startIndex + 1
                };
            }
            else
            {
                return new int[]
                {
                    startIndex + 0, startIndex + 2, startIndex + 3,
                    startIndex + 0, startIndex + 3, startIndex + 1
                };
            }
        }

        #endregion

        #endregion

        #region BlockPreviewTexture

        private static Mesh _previewTextureMesh = null;
        public static Texture GetBlockPreviewTexture(
            BlockTemplateRuntimeData blockTemplate,
            BlockTemplatePreviewSetting previewSetting,
            Material material,
            Vector2 textureSize,
            Vector3 cameraPos,
            Quaternion cameraRotation)
        {
            var worldBlockPivotPos = Vector3.zero -
                new Vector3(previewSetting.BlockSize.x, previewSetting.BlockSize.y, previewSetting.BlockSize.z) / 2f;
            if (_previewTextureMesh == null)
            {
                _previewTextureMesh = new Mesh();
            }

            CreatePreviewBlockMesh(
                _previewTextureMesh,
                blockTemplate,
                previewSetting,
                worldBlockPivotPos);

            return EditorPreviewUtility.GenPreviewTexture(
                _previewTextureMesh,
                material,
                textureSize,
                cameraPos,
                cameraRotation,
                Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));
        }

        #endregion
    }
}
