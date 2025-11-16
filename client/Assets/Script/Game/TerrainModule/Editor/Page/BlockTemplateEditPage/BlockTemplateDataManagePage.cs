using Framework.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class BlockTemplateDataManagePage : BlockTemplateEditBasePage
    {
        private struct CreateDataDescription
        {
            public string Name;
            public Texture2D TileMap;
            public Vector2Int Tiling;
            public Shader Shader;
        }

        //New Data
        private CreateDataDescription _createDataDescription;

        //Select Load Data
        private string[] _editDataNames;
        private int _curSelectEditDataIndex = -1;

        public override string Name => TerrainEditorDefine.BlockTemplateEditPageToName[(int)BlockTemplateEditPageType.DataManage];

        public BlockTemplateDataManagePage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override void OnEnable()
        {
            RefreshEditDataList();
        }


        public override void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
            {
                DrawGUI_CreateDataDescription();
                GUILayout.Space(5f);
                DrawGUI_LoadData();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGUI_CreateDataDescription()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(_editorData.EditorWindow.position.width / 2));
            {
                EditorGUILayout.LabelField("新建檔案");
                _createDataDescription.Name = EditorGUILayout.TextField("名稱", _createDataDescription.Name);
                _createDataDescription.TileMap =
                    (Texture2D)EditorGUILayout.ObjectField(
                        "TileMap:",
                        _createDataDescription.TileMap,
                        typeof(Texture2D),
                        false);
                _createDataDescription.Tiling = EditorGUILayout.Vector2IntField("Tiling:", _createDataDescription.Tiling);
                _createDataDescription.Shader =
                    (Shader)EditorGUILayout.ObjectField(
                        "Shader:",
                        _createDataDescription.Shader,
                        typeof(Shader),
                        false);

                if (GUILayout.Button("新建檔案"))
                {
                    if (CreateNewEditData())
                    {
                        ChangeToPage(BlockTemplateEditPageType.Edit);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGUI_LoadData()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(_editorData.EditorWindow.position.width / 2));
            {
                EditorGUILayout.LabelField("讀取檔案");
                if (GUILayout.Button("刷新檔案列表"))
                {
                    RefreshEditDataList();
                }

                if (_editDataNames.Length > 0)
                {
                    _curSelectEditDataIndex = Math.Clamp(_curSelectEditDataIndex, 0, _editDataNames.Length);
                    _curSelectEditDataIndex = EditorGUILayout.Popup(_curSelectEditDataIndex, _editDataNames);

                    if (GUILayout.Button("讀取檔案"))
                    {
                        if (_editorData.LoadBlockTemplateData(_editDataNames[_curSelectEditDataIndex]))
                        {
                            ChangeToPage(BlockTemplateEditPageType.Edit);
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private bool CreateNewEditData()
        {
            //Name Check
            if (string.IsNullOrEmpty(_createDataDescription.Name))
            {
                EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, "名稱不能為空白", TerrainEditorDefine.Dialog_Ok_Confirm);
                return false;
            }
            var existNames = TerrainEditorUtility.GetBlockTemplateEditDataNames();
            for (int i = 0; i < existNames.Length; i++)
            {
                if (_createDataDescription.Name == existNames[i])
                {
                    EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, "已有相同名稱", TerrainEditorDefine.Dialog_Ok_Confirm);
                    return false;
                }
            }

            var newEditData = new BlockTemplateEditData(
                _createDataDescription.TileMap,
                _createDataDescription.Tiling,
                _createDataDescription.Shader);
            newEditData.name = _createDataDescription.Name;
            AssetDatabase.CreateAsset(newEditData, Path.Combine(TerrainEditorDefine.EditBlockTemplateDataFolderPath, newEditData.name) + ".asset");
            AssetDatabase.Refresh();
            return _editorData.LoadBlockTemplateData(newEditData.name);
        }

        private void RefreshEditDataList()
        {
            _editDataNames = TerrainEditorUtility.GetBlockTemplateEditDataNames();
        }
    }
}
