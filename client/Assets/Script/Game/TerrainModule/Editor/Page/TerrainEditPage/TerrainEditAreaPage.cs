using Framework.Editor;
using Framework.Editor.Utility;
using TerrainModule.Editor;
using UnityEditor;
using UnityEngine;
using static TerrainModule.TerrainDefine;
using Extension;
using System;

namespace TerrainModule
{
    public class TerrainEditAreaPage : TerrainEditBasePage
    {
        private enum AddType
        {
            None,
            AreaPointGroup,
            Area
        }

        public override string Name => TerrainEditorDefine.TerrainEditPageToName[(int)TerrainEditPageType.EditArea];

        private TerrainEditRuntimeData CurEditRuntimeData => _editorData.CurTerrainEditRuntimeData;

        private int _curEditAreaGroupId = -1;
        private AreaGroupEditRuntimeData _curEdiAreaGroupData = null;

        private Vector2 _areaGroupScrollPos = Vector2.zero;
        private int _removeAreaGroupIndex = -1;

        private Vector2 _areaScrollPos = Vector2.zero;
        private int _removeAreaIndex = -1;

        private int _editDistance = 0;
        private AddType _addType = AddType.None;
        private int _addId = 1;

        private GUIStyle _areaGroupGUISelected;
        private GUIStyle _areaGroupGUIUnselect;

        private GUIStyle _areaGUI;

        private Color _areaGroupSelectedColor = new Color(0, 0.5f, 1f, 0.8f);
        private Color _areaGroupUnSelectColor = new Color(0, 0.25f, 0.5f, 0.3f);
        private Color _areaColor = new Color(1, 0.5f, 1, 0.8f);
        private Color _areaSphereColor = new Color(1, 0.5f, 1, 0.5f);

        public TerrainEditAreaPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override void OnEnable()
        {
            _areaGroupGUISelected = new GUIStyle();
            _areaGroupGUISelected.alignment = TextAnchor.MiddleCenter;
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, _areaGroupSelectedColor);
            t.Apply();
            _areaGroupGUISelected.normal.background = t;
            _areaGroupGUISelected.normal.textColor = Color.white;

            _areaGroupGUIUnselect = new GUIStyle();
            _areaGroupGUIUnselect.alignment = TextAnchor.MiddleCenter;
            t = new Texture2D(1, 1);
            t.SetPixel(0, 0, _areaGroupUnSelectColor);
            t.Apply();
            _areaGroupGUIUnselect.normal.background = t;
            _areaGroupGUIUnselect.normal.textColor = Color.white;

            _areaGUI = new GUIStyle();
            _areaGUI.alignment = TextAnchor.MiddleCenter;
            t = new Texture2D(1, 1);
            t.SetPixel(0, 0, _areaColor);
            t.Apply();
            _areaGUI.normal.background = t;
            _areaGUI.normal.textColor = Color.white;

            _addType = AddType.None;
            _curEdiAreaGroupData = null;
            if (CurEditRuntimeData != null && CurEditRuntimeData.TryGetAreaGroup(_curEditAreaGroupId, out var data))
                _curEdiAreaGroupData = data;
        }

        public override void OnGUI()
        {
            if (CurEditRuntimeData == null)
                return;

            DrawEditorSetting();
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                DrawAreaGroupData();
                GUILayout.Space(5f);
                DrawCurEditAreaGroup();
            }
            EditorGUILayout.EndHorizontal();
        }

        public override void OnSceneGUI()
        {
            if (CurEditRuntimeData == null)
                return;

            if (_addType != AddType.None)
            {
                Event currentEvent = Event.current;
                Vector3 mousePosition = currentEvent.mousePosition;
                Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                if (CurEditRuntimeData.RaycastBlockMesh(ray, _editDistance, out var hit))
                {
                    if (_addType == AddType.AreaPointGroup)
                        HandlesUtil.Sphere(hit.point, 0.2f, _areaGroupSelectedColor);
                    else if (_addType == AddType.Area)
                        HandlesUtil.Sphere(hit.point, 0.2f, _areaColor);

                    Handles.BeginGUI();
                    var guiPoint = HandleUtility.WorldToGUIPointWithDepth(hit.point);
                    EditorGUI.LabelField(
                        new Rect(guiPoint + new Vector3(-60, 20),
                        new Vector2(120, 20)), $"{hit.point}",
                        _addType == AddType.AreaPointGroup ? _areaGroupGUISelected : _areaGUI);
                    Handles.EndGUI();

                    if (currentEvent.type == EventType.MouseMove)
                    {
                        SceneView.RepaintAll();
                    }

                    if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                    {
                        currentEvent.Use();
                        if (_addType == AddType.AreaPointGroup)
                        {
                            if (CurEditRuntimeData.AddAreaGroup(_addId, hit.point))
                            {
                                SceneView.RepaintAll();
                                Repaint();
                                _addType = AddType.None;
                                if (CurEditRuntimeData.TryGetAreaGroup(_addId, out var data))
                                {
                                    _curEditAreaGroupId = data.Id;
                                    _curEdiAreaGroupData = data;
                                }
                            }
                            else
                            {
                                EditorUtility.DisplayDialog(
                                  TerrainEditorDefine.Dialog_Title_Error,
                                  "新增群組失敗, 已有相同群組Id",
                                  TerrainEditorDefine.Dialog_Ok_Confirm);
                            }
                        }
                        else if (_addType == AddType.Area)
                        {
                            if (_curEdiAreaGroupData == null)
                            {
                                EditorUtility.DisplayDialog(
                                    TerrainEditorDefine.Dialog_Title_Error,
                                    "新增區域失敗, 無選擇任何群組",
                                    TerrainEditorDefine.Dialog_Ok_Confirm);
                            }
                            else
                            {
                                if (_curEdiAreaGroupData.AddArea(_addId, hit.point))
                                {
                                    _addId++;
                                    SceneView.RepaintAll();
                                    Repaint();
                                }
                                else
                                {
                                    EditorUtility.DisplayDialog(
                                        TerrainEditorDefine.Dialog_Title_Error,
                                        "新增區域失敗, 已有相同區域Id",
                                        TerrainEditorDefine.Dialog_Ok_Confirm);
                                }
                            }
                        }
                    }
                }
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }

            if (_curEdiAreaGroupData != null)
            {
                EditorGUI.BeginChangeCheck();
                var newCenter = Handles.PositionHandle(_curEdiAreaGroupData.Bounds.center, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    var diff = newCenter - _curEdiAreaGroupData.Bounds.center;
                    _curEdiAreaGroupData.Translate(diff);
                    Repaint();
                }
                Handles.color = Color.blue;
                Handles.DrawWireCube(_curEdiAreaGroupData.Bounds.center, _curEdiAreaGroupData.Bounds.size);
                HandlesUtil.Sphere(_curEdiAreaGroupData.Bounds.center, 0.2f, _areaGroupSelectedColor);
                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < _curEdiAreaGroupData.AreaList.Count; i++)
                {
                    var area = _curEdiAreaGroupData.AreaList[i];

                    EditorGUI.BeginChangeCheck();
                    area.WorldPoint = Handles.PositionHandle(area.WorldPoint, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                        _curEdiAreaGroupData.RefreshBounds();
                    HandlesUtil.Sphere(area.WorldPoint, 0.2f, _areaColor);
                    if (area.AreaType == AreaType.Sphere)
                    {
                        HandlesUtil.Sphere(area.WorldPoint, area.Radius, _areaSphereColor);
                    }
                }
                if (EditorGUI.EndChangeCheck())
                    Repaint();
            }

            Handles.BeginGUI();
            var sceneBounds = SceneView.lastActiveSceneView.position;
            for (int i = 0; i < CurEditRuntimeData.AreaGroupEditDataList.Count; i++)
            {
                var areaGroup = CurEditRuntimeData.AreaGroupEditDataList[i];
                var guiPoint = HandleUtility.WorldToGUIPointWithDepth(areaGroup.Bounds.center);
                if (guiPoint.z <= 0)
                    continue;
                if (!sceneBounds.Contains(new Vector2(guiPoint.x, guiPoint.y) + sceneBounds.position))
                    continue;

                EditorGUI.LabelField(new Rect(guiPoint + new Vector3(-40, -40), new Vector2(80, 20)), $"G{areaGroup.Id}", GetAreaGroupGUIStyle(areaGroup.Id == _curEditAreaGroupId));
            }
            if (_curEdiAreaGroupData != null)
            {
                for (int i = 0; i < _curEdiAreaGroupData.AreaList.Count; i++)
                {
                    var area = _curEdiAreaGroupData.AreaList[i];
                    var guiPoint = HandleUtility.WorldToGUIPointWithDepth(area.WorldPoint);
                    if (guiPoint.z <= 0)
                        continue;
                    if (!sceneBounds.Contains(new Vector2(guiPoint.x, guiPoint.y) + sceneBounds.position))
                        continue;

                    EditorGUI.LabelField(new Rect(guiPoint + new Vector3(-40, 20), new Vector2(80, 20)), $"A{area.Id}", _areaGUI);
                    EditorGUI.LabelField(new Rect(guiPoint + new Vector3(-50, 40), new Vector2(100, 20)), $"{area.WorldPoint}", _areaGUI);
                    if (area.AreaType == AreaType.Sphere)
                    {
                        EditorGUI.LabelField(new Rect(guiPoint + new Vector3(-40, 60), new Vector2(20, 20)), $"R:", _areaGUI);
                        area.Radius = EditorGUI.FloatField(new Rect(guiPoint + new Vector3(-20, 60), new Vector2(60, 20)), area.Radius, _areaGUI);
                    }
                }
            }
            Handles.EndGUI();
        }

        private void DrawEditorSetting()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                _editDistance = EditorGUILayout.IntSlider(
                    "RayDistance:",
                    _editDistance,
                    128,
                    Mathf.Max(128, Mathf.Min((int)CurEditRuntimeData.TerrainSize().y, 512)));
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAreaGroupData()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(200));
            {
                EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(200));
                {
                    if (_addType == AddType.AreaPointGroup)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Id:", GUILayout.MaxWidth(100));
                        _addId = Math.Max(1, EditorGUILayout.IntField(_addId, GUILayout.MaxWidth(100)));
                        EditorGUILayout.EndHorizontal();
                        if (GUILayout.Button("取消"))
                            _addType = AddType.None;
                    }
                    else
                    {
                        if (GUILayout.Button("新增"))
                        {
                            _addType = AddType.AreaPointGroup;
                            _addId = CurEditRuntimeData.GetAreaGroupNextId();
                        }
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
                if (CurEditRuntimeData.AreaGroupEditDataList.TryGet(_removeAreaGroupIndex, out var data))
                {
                    //刪除當前編輯的要清空
                    if (data.Id == _curEditAreaGroupId)
                    {
                        _curEditAreaGroupId = -1;
                        _curEdiAreaGroupData = null;
                    }
                    CurEditRuntimeData.AreaGroupEditDataList.RemoveAt(_removeAreaGroupIndex);
                }
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
                    SceneViewLookAt(groupData.Bounds.center);
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
                if (_addType == AddType.Area)
                {
                    _addId = Math.Max(1, EditorGUILayout.IntField("Id:", _addId));
                    if (GUILayout.Button("取消"))
                        _addType = AddType.None;
                }
                else
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("新增"))
                    {
                        _addType = AddType.Area;
                        _addId = _curEdiAreaGroupData.GetAreaNextId();
                    }
                }
                EditorGUILayout.EndHorizontal();

                _areaScrollPos = EditorGUILayout.BeginScrollView(_areaScrollPos);
                {
                    EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
                    {
                        for (int i = 0; i < _curEdiAreaGroupData.AreaList.Count; i++)
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                DrawAreaCell(_curEdiAreaGroupData.AreaList[i], i);
                                GUILayout.Space(2f);
                            }
                            if (EditorGUI.EndChangeCheck())
                            {
                                _curEdiAreaGroupData.RefreshBounds();
                                SceneView.RepaintAll();
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();

            if (_removeAreaIndex > -1)
            {
                _curEdiAreaGroupData.RemoveAreaByIndex(_removeAreaIndex);
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
                    SceneViewLookAt(areaData.WorldPoint);
                }
            }
        }

        private void SceneViewLookAt(Vector3 worldPosition)
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
                sceneView.Frame(new Bounds(worldPosition, Vector3.one * 10), false);
        }

        private GUIStyle GetAreaGroupGUIStyle(bool select)
        {
            return select ? _areaGroupGUISelected : _areaGroupGUIUnselect;
        }
    }
}
