using Framework.Editor;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class TerrainDataManagePage : TerrainEditBasePage
    {
        private struct CreateDataDescription
        {
            public string Name;
            public Vector3Int BlockSize;
            public Vector3Int ChunkBlockNum;
            public Vector3Int ChunkNum;
            public BlockTemplateEditData BlockTemplateEditData;
            public EnvironmentTemplateEditData EnvironmentTemplateEditData;
        }

        //New Data
        private CreateDataDescription _createDataDescription;

        //Select Terrain Load Data
        private string[] _editDataNames;
        private int _curSelectEditDataIndex = -1;

        public TerrainDataManagePage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override string Name => TerrainEditorDefine.TerrainEditPageToName[(int)TerrainEditPageType.DataManage];

        public override void OnEnable()
        {
            RefreshTerrainEditDataList();
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

        public override void OnSceneGUI()
        {
            TerrainEditorUtility.HandleDrawChunk(
                _createDataDescription.BlockSize,
                _createDataDescription.ChunkBlockNum,
                _createDataDescription.ChunkNum,
                Color.white);
        }

        private void DrawGUI_CreateDataDescription()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(_editorData.EditorWindow.position.width / 2));
            {
                EditorGUILayout.LabelField("新建檔案");
                _createDataDescription.Name = EditorGUILayout.TextField("名稱", _createDataDescription.Name);
                _createDataDescription.BlockSize = ClampValidValue(EditorGUILayout.Vector3IntField("BlockSize", _createDataDescription.BlockSize));
                _createDataDescription.ChunkBlockNum = ClampValidValue(EditorGUILayout.Vector3IntField("ChunkBlockNum", _createDataDescription.ChunkBlockNum));
                _createDataDescription.ChunkNum = ClampValidValue(EditorGUILayout.Vector3IntField("ChunkNum", _createDataDescription.ChunkNum));
                _createDataDescription.BlockTemplateEditData =
                    (BlockTemplateEditData)EditorGUILayout.ObjectField(
                        "地格範本:",
                        _createDataDescription.BlockTemplateEditData,
                        typeof(BlockTemplateEditData),
                        false);
                _createDataDescription.EnvironmentTemplateEditData =
                    (EnvironmentTemplateEditData)EditorGUILayout.ObjectField(
                        "環境範本:",
                        _createDataDescription.EnvironmentTemplateEditData,
                        typeof(EnvironmentTemplateEditData),
                        false);

                if (GUILayout.Button("新建檔案"))
                {
                    if (CreateNewEditData())
                    {
                        ChangeToPage(TerrainEditPageType.EditTerrain);
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
                    RefreshTerrainEditDataList();
                }

                if (_editDataNames.Length > 0)
                {
                    _curSelectEditDataIndex = Math.Clamp(_curSelectEditDataIndex, 0, _editDataNames.Length);
                    _curSelectEditDataIndex = EditorGUILayout.Popup(_curSelectEditDataIndex, _editDataNames);

                    if (GUILayout.Button("讀取檔案"))
                    {
                        if (_editorData.LoadTerrainData(_editDataNames[_curSelectEditDataIndex]))
                        {
                            ChangeToPage(TerrainEditPageType.EditTerrain);
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private Vector3Int ClampValidValue(Vector3Int vector)
        {
            return new Vector3Int(
                Math.Clamp(vector.x, 1, int.MaxValue),
                Math.Clamp(vector.y, 1, int.MaxValue),
                Math.Clamp(vector.z, 1, int.MaxValue));
        }

        private bool CreateNewEditData()
        {
            //Name Check
            if (string.IsNullOrEmpty(_createDataDescription.Name))
            {
                EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, "名稱不能為空白", TerrainEditorDefine.Dialog_Ok_Confirm);
                return false;
            }
            var existNames = TerrainEditorUtility.GetTerrainEditDataNames();
            for (int i = 0; i < existNames.Length; i++)
            {
                if (_createDataDescription.Name == existNames[i])
                {
                    EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, "已有相同名稱", TerrainEditorDefine.Dialog_Ok_Confirm);
                    return false;
                }
            }

            //Block and Chunk Check
            _createDataDescription.BlockSize = ClampValidValue(_createDataDescription.BlockSize);
            _createDataDescription.ChunkBlockNum = ClampValidValue(_createDataDescription.ChunkBlockNum);
            _createDataDescription.ChunkNum = ClampValidValue(_createDataDescription.ChunkNum);

            if (!Directory.Exists(TerrainEditorDefine.EditTerrainDataFolderPath))
                Directory.CreateDirectory(TerrainEditorDefine.EditTerrainDataFolderPath);

            var newEditData = new TerrainEditData(
                _createDataDescription.BlockSize,
                _createDataDescription.ChunkBlockNum,
                _createDataDescription.ChunkNum,
                _createDataDescription.BlockTemplateEditData,
                _createDataDescription.EnvironmentTemplateEditData);
            newEditData.name = _createDataDescription.Name;
            AssetDatabase.CreateAsset(newEditData, Path.Combine(TerrainEditorDefine.EditTerrainDataFolderPath, newEditData.name) + ".asset");
            AssetDatabase.Refresh();
            return _editorData.LoadTerrainData(newEditData.name);
        }

        private void RefreshTerrainEditDataList()
        {
            _editDataNames = TerrainEditorUtility.GetTerrainEditDataNames();
        }
    }
}
