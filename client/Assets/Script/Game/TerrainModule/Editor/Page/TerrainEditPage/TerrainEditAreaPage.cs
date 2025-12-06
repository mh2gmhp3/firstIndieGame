using Framework.Editor;
using TerrainModule.Editor;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UIElements;
using static TerrainModule.TerrainDefine;

namespace TerrainModule
{
    public class TerrainEditAreaPage : TerrainEditBasePage
    {
        public override string Name => TerrainEditorDefine.TerrainEditPageToName[(int)TerrainEditPageType.EditArea];

        private TerrainEditRuntimeData CurEditRuntimeData => _editorData.CurTerrainEditRuntimeData;

        private int _curEditAreaGroupId = -1;
        private AreaGroupEditRuntimeData _curEdiAreaGroupData = null;

        private Vector2 _areaGroupScrollPos = Vector2.zero;
        private int _removeAreaGroupIndex = -1;

        private Vector2 _areaScrollPos = Vector2.zero;
        private int _removeAreaIndex = -1;

        public TerrainEditAreaPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override void OnEnable()
        {
            _curEdiAreaGroupData = null;
            if (CurEditRuntimeData != null && CurEditRuntimeData.TryGetAreaGroup(_curEditAreaGroupId, out var data))
                _curEdiAreaGroupData = data;
        }

        public override void OnGUI()
        {
            if (CurEditRuntimeData == null)
                return;

            EditorGUILayout.BeginHorizontal();
            {
                DrawAreaGroupData();
                GUILayout.Space(5f);
                DrawCurEditAreaGroup();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAreaGroupData()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(200));
            {
                EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(200));
                {
                    if (GUILayout.Button("新增"))
                    {
                        CurEditRuntimeData.AddAreaGroup();
                    }
                }
                EditorGUILayout.EndVertical();

                _areaGroupScrollPos = EditorGUILayout.BeginScrollView(_areaGroupScrollPos, CommonGUIStyle.Default_Box);
                {
                    EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(170));
                    {
                        for (int i = 0; i < CurEditRuntimeData.AreaGroupEditDataList.Count; i++)
                        {
                            DrawAreaGroupCell(CurEditRuntimeData.AreaGroupEditDataList[i], i);
                            GUILayout.Space(2f);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();

            if (_removeAreaGroupIndex > -1)
            {
                CurEditRuntimeData.AreaGroupEditDataList.RemoveAt(_removeAreaGroupIndex);
                _removeAreaGroupIndex = -1;
            }
        }

        private void DrawAreaGroupCell(AreaGroupEditRuntimeData groupData, int index)
        {
            float maxWidth = 145;
            EditorGUILayout.BeginVertical(CommonGUIStyle.SelectableBlueBox(groupData.Id == _curEditAreaGroupId), GUILayout.MaxWidth(maxWidth));
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Id:{groupData.Id}", GUILayout.MaxWidth(maxWidth));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X"))
                {
                    _removeAreaGroupIndex = index;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField(groupData.Description, GUILayout.MaxWidth(maxWidth));
            }
            EditorGUILayout.EndVertical();
            var lastRect = GUILayoutUtility.GetLastRect();
            Event current = Event.current;
            if (lastRect.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    _curEditAreaGroupId = groupData.Id;
                    _curEdiAreaGroupData = groupData;
                    Repaint();
                }
            }
        }

        private void DrawCurEditAreaGroup()
        {
            if (_curEdiAreaGroupData == null)
            {
                EditorGUILayout.LabelField("沒有選擇區域群組");
                return;
            }

            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.LabelField($"Id:{_curEdiAreaGroupData.Id}");
                EditorGUILayout.BeginHorizontal();
                _curEdiAreaGroupData.Description = EditorGUILayout.TextField(_curEdiAreaGroupData.Description);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("新增"))
                {
                    _curEdiAreaGroupData.AddArea();
                }
                EditorGUILayout.EndHorizontal();

                _areaScrollPos = EditorGUILayout.BeginScrollView(_areaScrollPos);
                {
                    EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
                    {
                        for (int i = 0; i < _curEdiAreaGroupData.AreaList.Count; i++)
                        {
                            DrawAreaCell(_curEdiAreaGroupData.AreaList[i], i);
                            GUILayout.Space(2f);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();

            if (_removeAreaIndex > -1)
            {
                _curEdiAreaGroupData.AreaList.RemoveAt(_removeAreaIndex);
                _removeAreaIndex = -1;
            }
        }

        private void DrawAreaCell(AreaEditRuntimeData areaData, int index)
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Id:{areaData.Id}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X"))
                {
                    _removeAreaIndex = index;
                }
                EditorGUILayout.EndHorizontal();
                areaData.AreaType = (AreaType)EditorGUILayout.EnumPopup("區域類型", areaData.AreaType);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"世界座標");
                areaData.WorldPoint = EditorGUILayout.Vector3Field("", areaData.WorldPoint);
                EditorGUILayout.EndHorizontal();
                if (areaData.AreaType == AreaType.Sphere)
                {
                    areaData.Radius = EditorGUILayout.FloatField("半徑", areaData.Radius);
                }
            }
            EditorGUILayout.EndVertical();
            var lastRect = GUILayoutUtility.GetLastRect();
            Event current = Event.current;
            if (lastRect.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    var sceneView = SceneView.lastActiveSceneView;
                    if (sceneView != null)
                        sceneView.Frame(new Bounds(areaData.WorldPoint, Vector3.one * 10), false);
                }
            }
        }
    }
}
