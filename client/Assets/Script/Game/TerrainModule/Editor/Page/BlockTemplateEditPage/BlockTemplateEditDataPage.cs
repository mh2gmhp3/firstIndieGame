using Framework.Editor;
using Framework.Editor.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static TerrainModule.Editor.TerrainEditorManager;

namespace TerrainModule.Editor
{
    public class BlockTemplateEditDataPage : BlockTemplateEditBasePage
    {
        public override string Name => TerrainEditorDefine.BlockTemplateEditPageToName[(int)BlockTemplateEditPageType.Edit];

        private bool _enablePreviewSettingPanel = false;
        private BlockTemplatePreviewSetting _previewSetting = new BlockTemplatePreviewSetting();

        private Vector2 _blockTemplateScrollPos = Vector2.zero;

        private BlockTemplateEditRuntimeData CurEditRuntimeData => _editorData.CurBlockTemplateEditRuntimeData;

        public BlockTemplateEditDataPage(TerrainEditorData editorData) : base(editorData)
        {
            editorData.BlockTemplatePreviewSetting = _previewSetting;
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
                CurEditRuntimeData.TileMap =
                    (Texture2D)EditorGUILayout.ObjectField(
                        "TileMap:",
                        CurEditRuntimeData.TileMap,
                        typeof(Texture2D),
                        false);
                CurEditRuntimeData.Tiling = EditorGUILayout.Vector2Field("Tiling:", CurEditRuntimeData.Tiling);
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
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(_editorData.EditorWindow.position.width / 2));
            {
                EditorGUILayout.LabelField("預覽參數");
                _previewSetting.BlockSize = EditorGUILayout.Vector3IntField("BlockSize", _previewSetting.BlockSize);
                Vector4 newYTopValue = _previewSetting.YTopValue;
                Vector4 newYBottomValue = _previewSetting.YBottomValue;
                EditorGUILayout.LabelField("Y高低值");
                EditorGUI.BeginChangeCheck();
                {
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
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBlockTemplateList()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(_editorData.EditorWindow.position.width / 2));
            {
                if (GUILayout.Button("新增"))
                {
                    var id = CurEditRuntimeData.AddBlockData();
                    SelectPreviewBlock(id);
                }

                _blockTemplateScrollPos = EditorGUILayout.BeginScrollView(_blockTemplateScrollPos, CommonGUIStyle.Default_Box);
                {
                    for (int i = 0; i < CurEditRuntimeData.BlockTemplateDataList.Count; i++)
                    {
                        DrawBlockTemplateScrollCell(CurEditRuntimeData.BlockTemplateDataList[i]);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBlockTemplateScrollCell(BlockTemplateRuntimeData data)
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.LabelField($"Id:{data.Id}");
                TerrainEditorUtility.DrawTiling(data, false);
                if (GUILayout.Button("選擇"))
                {
                    SelectPreviewBlock(data.Id);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedBlock()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(_editorData.EditorWindow.position.width / 2));
            {
                if (CurEditRuntimeData.TryGetBlockData(_previewSetting.PreviewId, out var blockTemplate))
                {
                    EditorGUILayout.LabelField("選擇方塊");
                    EditorGUILayout.LabelField($"Id:{blockTemplate.Id}");
                    EditorGUI.BeginChangeCheck();
                    TerrainEditorUtility.DrawTiling(blockTemplate, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _editorData.TerrainEditorMgr.RefreshBlockTemplatePreview();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("未選擇方塊");
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void SelectPreviewBlock(int id)
        {
            _previewSetting.PreviewId = id;
            _editorData.TerrainEditorMgr.RefreshBlockTemplatePreview();
        }
    }
}
