using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class TerrainCreateDataPage : TerrainEditorBasePage
    {
        private struct CreateDataDescription
        {
            public string Name;
            public Vector3Int BlockSize;
            public Vector3Int ChunkBlockNum;
            public Vector3Int ChunkNum;
            public Material TerrainMaterial;
        }

        //New Data
        private CreateDataDescription _createDataDescription;

        public TerrainCreateDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override string Name => TerrainEditorDefine.TerrainEditPageToName[(int)TerrainEditPageType.Create];

        public override void OnEnable()
        {

        }

        public override void OnGUI()
        {
            DrawGUI_CreateDataDescription();
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
            _createDataDescription.Name = EditorGUILayout.TextField("名稱", _createDataDescription.Name);
            _createDataDescription.BlockSize = ClampValidValue(EditorGUILayout.Vector3IntField("BlockSize", _createDataDescription.BlockSize));
            _createDataDescription.ChunkBlockNum = ClampValidValue(EditorGUILayout.Vector3IntField("ChunkBlockNum", _createDataDescription.ChunkBlockNum));
            _createDataDescription.ChunkNum = ClampValidValue(EditorGUILayout.Vector3IntField("ChunkNum", _createDataDescription.ChunkNum));
            _createDataDescription.TerrainMaterial =
                (Material)EditorGUILayout.ObjectField(
                    "材質:",
                    _createDataDescription.TerrainMaterial,
                    typeof(Material),
                    false);

            if (GUILayout.Button("新增檔案"))
            {
                CreateNewEditData();
            }
        }

        private Vector3Int ClampValidValue(Vector3Int vector)
        {
            return new Vector3Int(
                Math.Clamp(vector.x, 1, int.MaxValue),
                Math.Clamp(vector.y, 1, int.MaxValue),
                Math.Clamp(vector.z, 1, int.MaxValue));
        }

        private void CreateNewEditData()
        {
            //Name Check
            if (string.IsNullOrEmpty(_createDataDescription.Name))
            {
                EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, "名稱不能為空白", TerrainEditorDefine.Dialog_Ok_Confirm);
                return;
            }
            var existNames = TerrainEditorUtility.GetTerrainEditDataNames();
            for (int i = 0; i < existNames.Length; i++)
            {
                if (_createDataDescription.Name == existNames[i])
                {
                    EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, "已有相同名稱", TerrainEditorDefine.Dialog_Ok_Confirm);
                    return;
                }
            }

            //Block and Chunk Check
            _createDataDescription.BlockSize = ClampValidValue(_createDataDescription.BlockSize);
            _createDataDescription.ChunkBlockNum = ClampValidValue(_createDataDescription.ChunkBlockNum);
            _createDataDescription.ChunkNum = ClampValidValue(_createDataDescription.ChunkNum);

            var newEditData = new TerrainEditData(
                _createDataDescription.BlockSize,
                _createDataDescription.ChunkBlockNum,
                _createDataDescription.ChunkNum,
                _createDataDescription.TerrainMaterial);
            newEditData.name = _createDataDescription.Name;
            AssetDatabase.CreateAsset(newEditData, Path.Combine(TerrainEditorDefine.EditTerrainDataFolderPath, newEditData.name) + ".asset");
            AssetDatabase.Refresh();
            _editorData.LoadTerrainData(newEditData.name);
        }
    }
}
