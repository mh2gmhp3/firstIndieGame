using System;
using UnityEditor;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class LoadDataPage : TerrainEditorPage
    {
        //Select Load Data
        private string[] _terrainEditDataNames;
        private int _curSelectEditDataIndex = -1;

        public LoadDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override string Name => TerrainEditorDefine.PageToName[(int)TerrainEditorPageType.Load];

        public override void OnEnable()
        {
            RefreshEditDataList();
        }

        public override void OnGUI()
        {
            DrawGUI_LoadData();
        }

        private void DrawGUI_LoadData()
        {
            if (GUILayout.Button("刷新檔案列表"))
            {
                RefreshEditDataList();
            }

            if (_terrainEditDataNames.Length > 0)
            {
                _curSelectEditDataIndex = Math.Clamp(_curSelectEditDataIndex, 0, _terrainEditDataNames.Length);
                _curSelectEditDataIndex = EditorGUILayout.Popup(_curSelectEditDataIndex, _terrainEditDataNames);

                if (GUILayout.Button("讀取檔案"))
                {
                    if (_editorData.LoadData(_terrainEditDataNames[_curSelectEditDataIndex]))
                    {
                        ChangeToPage(TerrainEditorPageType.Edit);
                    }
                }
            }
        }

        private void RefreshEditDataList()
        {
            _terrainEditDataNames = TerrainEditorWindow.GetEditDataFolderNames();
        }
    }
}
