using Framework.Editor;
using Framework.Editor.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static TerrainModule.Editor.TerrainEditorManager;

namespace TerrainModule.Editor
{
    public class BlockTemplateEditDataPage : BlockTemplateEditBasePage
    {
        public override string Name => TerrainEditorDefine.BlockTemplateEditPageToName[(int)BlockTemplateEditPageType.Edit];

        private BlockTemplatePreviewSetting _previewSetting = new BlockTemplatePreviewSetting();

        private Vector2 _blockTemplateScrollPos = Vector2.zero;
        private bool _scrollerShowTilingValue = false;
        private List<int> _removeBlockDataId = new List<int>();

        private Vector3Int _curEditFace = Vector3Int.zero;

        private BlockTemplateEditRuntimeData CurEditRuntimeData => _editorData.CurBlockTemplateEditRuntimeData;

        public BlockTemplateEditDataPage(TerrainEditorData editorData) : base(editorData)
        {
            editorData.BlockTemplatePreviewSetting = _previewSetting;
        }

        public override void OnDisable()
        {
            EditorPreviewUtility.Cleanup();
        }

        public override void OnGUI()
        {
            if (CurEditRuntimeData == null)
                return;
            EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
            {
                DrawCurEditData();
                GUILayout.Space(5f);
                DrawPreviewSetting();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
            {
                DrawBlockTemplateList();
                GUILayout.Space(5f);
                DrawSelectedBlock();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCurEditData()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(_editorData.EditorWindow.position.width / 2));
            {
                EditorGUILayout.TextField("名稱", CurEditRuntimeData.Name);
                EditorGUI.BeginChangeCheck();
                CurEditRuntimeData.TileMap =
                    (Texture2D)EditorGUILayout.ObjectField(
                        "TileMap:",
                        CurEditRuntimeData.TileMap,
                        typeof(Texture2D),
                        false);
                CurEditRuntimeData.Tiling = EditorGUILayout.Vector2IntField("Tiling:", CurEditRuntimeData.Tiling);
                CurEditRuntimeData.Shader =
                    (Shader)EditorGUILayout.ObjectField(
                        "Shader:",
                        CurEditRuntimeData.Shader,
                        typeof(Shader),
                        false);
                if (EditorGUI.EndChangeCheck())
                {
                    CurEditRuntimeData.RefreshMaterial();
                    _editorData.TerrainEditorMgr.RefreshBlockTemplatePreview();
                }

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("存檔"))
                    {
                        _editorData.SaveCurBlockTemplateData();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewSetting()
        {
            Vector4 newYTopValue = _previewSetting.YTopValue;
            Vector4 newYBottomValue = _previewSetting.YBottomValue;
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(_editorData.EditorWindow.position.width / 2));
            {
                EditorGUILayout.LabelField("預覽參數");
                EditorGUI.BeginChangeCheck();
                {
                    _previewSetting.BlockSize = EditorGUILayout.Vector3IntField("BlockSize", _previewSetting.BlockSize);
                    EditorGUILayout.LabelField("Y高低值");
                    EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                    {
                        EditorGUIUtil.MinMaxSlider("-z-x", ref newYBottomValue.x, ref newYTopValue.x, 0, 1);
                        GUILayout.Space(5f);
                        EditorGUIUtil.MinMaxSlider("-z+x", ref newYBottomValue.y, ref newYTopValue.y, 0, 1);
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                    {
                        EditorGUIUtil.MinMaxSlider("+z-x", ref newYBottomValue.z, ref newYTopValue.z, 0, 1);
                        GUILayout.Space(5f);
                        EditorGUIUtil.MinMaxSlider("+z+x", ref newYBottomValue.w, ref newYTopValue.w, 0, 1);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("填滿方塊"))
                        {
                            newYTopValue = Vector4.one;
                            newYBottomValue = Vector4.zero;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    _previewSetting.SetYValue(newYTopValue, newYBottomValue);
                    _editorData.TerrainEditorMgr.RefreshBlockTemplatePreview();
                    CurEditRuntimeData.MarkRefreshAllPreviewTexture();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBlockTemplateList()
        {
            var maxWidth = _scrollerShowTilingValue ? _editorData.EditorWindow.position.width / 2 : 190;
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(maxWidth));
            {
                if (GUILayout.Button("新增"))
                {
                    var id = CurEditRuntimeData.AddBlockData();
                    SelectPreviewBlock(id);
                }

                _scrollerShowTilingValue = EditorGUILayout.Toggle("顯示TilingValue:", _scrollerShowTilingValue);

                _blockTemplateScrollPos = EditorGUILayout.BeginScrollView(_blockTemplateScrollPos, CommonGUIStyle.Default_Box);
                {
                    for (int i = 0; i < CurEditRuntimeData.BlockTemplateDataList.Count; i++)
                    {
                        DrawBlockTemplateScrollCell(CurEditRuntimeData.BlockTemplateDataList[i]);
                        GUILayout.Space(2f);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();

            for (int i = 0; i < _removeBlockDataId.Count; i++)
            {
                CurEditRuntimeData.RemoveBlockData(_removeBlockDataId[i]);
            }
            _removeBlockDataId.Clear();
        }

        private void DrawBlockTemplateScrollCell(BlockTemplateRuntimeData data)
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.SelectableBlueBox(_previewSetting.PreviewId == data.Id));
            {
                EditorGUILayout.LabelField($"Id:{data.Id}");
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    var rect = GUILayoutUtility.GetRect(150, 150, GUILayout.Width(150), GUILayout.Height(150));
                    TerrainEditorUtility.DrawBlockTemplatePreview(
                        data,
                        _editorData.TerrainEditorMgr,
                        CurEditRuntimeData.Material,
                        _previewSetting.BlockSize,
                        rect);
                    if (_scrollerShowTilingValue)
                        TerrainEditorUtility.DrawTiling(data);
                }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("X", GUILayout.MaxWidth(50)))
                {
                    _removeBlockDataId.Add(data.Id);
                }
            }
            EditorGUILayout.EndVertical();
            var lastRect = GUILayoutUtility.GetLastRect();
            Event current = Event.current;
            if (lastRect.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    SelectPreviewBlock(data.Id);
                }
            }
        }

        private void DrawSelectedBlock()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                if (CurEditRuntimeData.TryGetBlockData(_previewSetting.PreviewId, out var blockTemplate))
                {
                    EditorGUILayout.LabelField("選擇方塊");
                    EditorGUILayout.LabelField($"Id:{blockTemplate.Id}");
                    EditorGUI.BeginChangeCheck();
                    _curEditFace = TerrainEditorUtility.DrawSelectableTiling(blockTemplate, _curEditFace);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Repaint();
                    }
                    EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                    {
                        var rect = GUILayoutUtility.GetRect(200, 200, GUILayout.Width(200), GUILayout.Height(200));
                        TerrainEditorUtility.DrawBlockTemplatePreview(
                            blockTemplate,
                            _editorData.TerrainEditorMgr,
                            CurEditRuntimeData.Material,
                            _previewSetting.BlockSize,
                            rect);
                        GUILayout.Space(10f);
                        DrawGridTexture();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField("未選擇方塊");
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGridTexture()
        {
            //Texture
            var rect = GUILayoutUtility.GetRect(CurEditRuntimeData.TileMap.width, CurEditRuntimeData.TileMap.height, GUILayout.MaxWidth(256), GUILayout.Height(256));
            GUI.DrawTexture(rect, CurEditRuntimeData.TileMap);
            Handles.BeginGUI();
            GUI.BeginClip(rect);
            Handles.color = Color.red;
            //Grid
            var gridSize = new Vector2(rect.width / CurEditRuntimeData.Tiling.x, rect.height / CurEditRuntimeData.Tiling.y);
            for (int i = 0; i <= CurEditRuntimeData.Tiling.x; i++)
            {
                var x = Mathf.Clamp(i * gridSize.x, 0.1f, rect.width - 0.1f);
                Handles.DrawLine(new Vector2(x, 0), new Vector2(x, rect.height));
            }
            for (int i = 0; i <= CurEditRuntimeData.Tiling.y; i++)
            {
                var y = Mathf.Clamp(i * gridSize.y, 0.1f, rect.height - 0.1f);
                Handles.DrawLine(new Vector2(0, y), new Vector2(rect.width, y));
            }
            //Change Tiling
            if (_curEditFace != Vector3Int.zero)
            {
                var current = Event.current;
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    var pos = new Vector2(current.mousePosition.x, Mathf.Abs(current.mousePosition.y - rect.height));
                    var normalUV = pos / rect.size;
                    var tilingValue = new Vector2(1f, 1f) / CurEditRuntimeData.Tiling;
                    var tilingUV = new Vector2(Mathf.Floor(normalUV.x / tilingValue.x) * tilingValue.x, Mathf.Floor(normalUV.y / tilingValue.y) * tilingValue.y);
                    if (CurEditRuntimeData.TryGetBlockData(_previewSetting.PreviewId, out var blockTemplate))
                    {
                        blockTemplate.SetTiling(_curEditFace, tilingUV);
                        _editorData.TerrainEditorMgr.RefreshBlockTemplatePreview();
                        blockTemplate.PreviewInfo.MarkRefreshTexture();
                        Repaint();
                    }
                }
            }
            GUI.EndClip();
            Handles.EndGUI();
        }

        private void SelectPreviewBlock(int id)
        {
            _previewSetting.PreviewId = id;
            _editorData.TerrainEditorMgr.RefreshBlockTemplatePreview();
            //有選擇清空當前編輯面
            _curEditFace = Vector3Int.zero;
            Repaint();
        }
    }
}
