using Framework.Editor;
using Framework.Editor.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class EnvironmentTemplateDataManagePage : EnvironmentTemplateBasePage
    {
        public override string Name => TerrainEditorDefine.EnvironmentTemplateEditPageToName[(int)EnvironmentTemplateEditPageType.DataManage];

        public EnvironmentTemplateDataManagePage(TerrainEditorData editorData) : base(editorData)
        {
        }

        private struct CreateDataDescription
        {
            public string Name;
        }

        //New Data
        private CreateDataDescription _createDataDescription;

        //Select Load Data
        private string[] _editDataNames;
        private int _curSelectEditDataIndex = -1;

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

                if (GUILayout.Button("新建檔案"))
                {
                    if (CreateNewEditData())
                    {
                        ChangeToPage(EnvironmentTemplateEditPageType.Edit);
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
                        if (_editorData.LoadEnvironmentTemplateData(_editDataNames[_curSelectEditDataIndex]))
                        {
                            ChangeToPage(EnvironmentTemplateEditPageType.Edit);
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
            var existNames = TerrainEditorUtility.GetEnvironmentTemplateEditDataNames();
            for (int i = 0; i < existNames.Length; i++)
            {
                if (_createDataDescription.Name == existNames[i])
                {
                    EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, "已有相同名稱", TerrainEditorDefine.Dialog_Ok_Confirm);
                    return false;
                }
            }

            if (!Directory.Exists(TerrainEditorDefine.EditEnvironmentTemplateDataFolderPath))
                Directory.CreateDirectory(TerrainEditorDefine.EditEnvironmentTemplateDataFolderPath);

            var newEditData = new EnvironmentTemplateEditData();
            newEditData.name = _createDataDescription.Name;
            AssetDatabase.CreateAsset(newEditData, Path.Combine(TerrainEditorDefine.EditEnvironmentTemplateDataFolderPath, newEditData.name) + ".asset");
            AssetDatabase.Refresh();
            return _editorData.LoadEnvironmentTemplateData(newEditData.name);
        }

        private void RefreshEditDataList()
        {
            _editDataNames = TerrainEditorUtility.GetEnvironmentTemplateEditDataNames();
        }
    }
}
