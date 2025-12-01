using Framework.Editor;
using Framework.Editor.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class TerrainEditEnvironmentPage : TerrainEditBasePage
    {
        private enum MouseFunction
        {
            AddEnvironment,
            DeleteEnvironment,
        }

        public override string Name => TerrainEditorDefine.TerrainEditPageToName[(int)TerrainEditPageType.EditEnvironment];

        private TerrainEditRuntimeData CurEditRuntimeData => _editorData.CurTerrainEditRuntimeData;
        private EnvironmentTemplateCategoryEditRuntimeData _curCategoryRuntimeData;
        private int _editDistance = 0;

        private string[] _categoryNames;
        private int _curSelectedCategoryIndex = -1;

        private Vector2 _instanceMeshScrollPos = Vector2.zero;

        private Vector2 _prefabScrollPos = Vector2.zero;

        private EnvironmentTemplatePreviewSetting _drawPreviewSetting = new EnvironmentTemplatePreviewSetting();

        public TerrainEditEnvironmentPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override void OnEnable()
        {
            RefreshCategoryNames();
            if (_curSelectedCategoryIndex == -1 && _categoryNames.Length > 0)
                _curSelectedCategoryIndex = 0;
            SelectCategory(_curSelectedCategoryIndex);
        }

        public override void OnDisable()
        {
            _editorData.TerrainEditorMgr.ClearTerrainEnvironmentDraw();
        }

        public override void OnGUI()
        {
            if (CurEditRuntimeData == null)
                return;

            DrawGUI_Category();
            GUILayout.Space(5f);
            DrawEditorSetting();
            GUILayout.Space(5f);
            DrawGUI_CategoryData();
        }

        public override void OnSceneGUI()
        {
            if (CurEditRuntimeData == null)
                return;

            Event currentEvent = Event.current;
            Vector3 mousePosition = currentEvent.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (CurEditRuntimeData.RaycastBlockMesh(ray, _editDistance, out var hit))
            {
                if (currentEvent.type == EventType.MouseMove)
                {
                    _editorData.TerrainEditorMgr.UpdateTerrainEnvironmentDraw(hit.point, Quaternion.identity, Vector3.one);
                    Repaint();
                }
                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                {
                    if (_curCategoryRuntimeData.TryGetName(_drawPreviewSetting.IsInstanceMesh, _drawPreviewSetting.Index, out var name))
                    {
                        if (CurEditRuntimeData.AddEnvironment(
                            _drawPreviewSetting.IsInstanceMesh,
                            _drawPreviewSetting.CategoryName,
                            name,
                            hit.point,
                            Quaternion.identity,
                            Vector3.one))
                        {
                            if (CurEditRuntimeData.TryGetId(hit.point, out int chunkId, out _))
                                _editorData.TerrainEditorMgr.RefreshChunkEnvironment(chunkId);
                        }
                    }
                    currentEvent.Use();
                }
            }
            if (_drawPreviewSetting.IsInstanceMesh)
            {
                //Mesh要強制呼叫Update繪製
                EditorApplication.QueuePlayerLoopUpdate();
            }
        }

        private void DrawGUI_Category()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                GUILayout.Space(5f);

                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    if (_categoryNames != null && _categoryNames.Length > 0)
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            _curSelectedCategoryIndex = EditorGUILayout.Popup(_curSelectedCategoryIndex, _categoryNames);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            SelectCategory(_curSelectedCategoryIndex);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
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

        private void DrawGUI_CategoryData()
        {
            if (_curCategoryRuntimeData == null)
                return;

            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                DrawGUI_Prefab();
                GUILayout.Space(5f);
                DrawGUI_InstanceMesh();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGUI_InstanceMesh()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.LabelField("InstanceMesh");
                _instanceMeshScrollPos = EditorGUILayout.BeginScrollView(_instanceMeshScrollPos, GUILayout.Height(220), GUILayout.MaxHeight(220));
                {
                    EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                    {
                        for (int i = 0; i < _curCategoryRuntimeData.InstanceMeshDataList.Count; i++)
                        {
                            var data = _curCategoryRuntimeData.InstanceMeshDataList[i];
                            DrawGUI_InstanceMeshCell(data, i);
                            GUILayout.Space(2f);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGUI_InstanceMeshCell(EnvironmentTemplateInstanceMeshEditRuntimeData data, int index)
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.SelectableBlueBox(
                _drawPreviewSetting.IsInstanceMesh && _drawPreviewSetting.Index == index),
                GUILayout.Width(160), GUILayout.MaxWidth(160));
            {
                EditorGUILayout.ObjectField(data.OriginObject.EditorInstance, typeof(GameObject), false);
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    var rect = GUILayoutUtility.GetRect(150, 150, GUILayout.Width(150), GUILayout.Height(150));
                    TerrainEditorUtility.DrawEnvironmentTemplatePreview(data, rect);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            var lastRect = GUILayoutUtility.GetLastRect();
            Event current = Event.current;
            if (lastRect.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    SelectInstanceMesh(index);
                }
            }
        }

        private void DrawGUI_Prefab()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.LabelField("Prefab");
                _prefabScrollPos = EditorGUILayout.BeginScrollView(_prefabScrollPos, GUILayout.Height(220), GUILayout.MaxHeight(220));
                {
                    EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                    {
                        for (int i = 0; i < _curCategoryRuntimeData.PrefabList.Count; i++)
                        {
                            var data = _curCategoryRuntimeData.PrefabList[i];
                            DrawGUI_PrefabCell(data, i);
                            GUILayout.Space(2f);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGUI_PrefabCell(EnvironmentTemplatePrefabEditRuntimeData data, int index)
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.SelectableBlueBox(
                !_drawPreviewSetting.IsInstanceMesh && _drawPreviewSetting.Index == index),
                GUILayout.Width(160),
                GUILayout.MaxWidth(160));
            {
                EditorGUILayout.ObjectField(data.Prefab.EditorInstance, typeof(GameObject), false);
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    var rect = GUILayoutUtility.GetRect(150, 150, GUILayout.Width(150), GUILayout.Height(150));
                    TerrainEditorUtility.DrawEnvironmentTemplatePreview(data, rect);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            var lastRect = GUILayoutUtility.GetLastRect();
            Event current = Event.current;
            if (lastRect.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    SelectPrefab(index);
                }
            }
        }

        private void SelectInstanceMesh(int index)
        {
            _drawPreviewSetting.IsInstanceMesh = true;
            _drawPreviewSetting.Index = index;
            Repaint();
            _editorData.TerrainEditorMgr.SetTerrainEnvironmentDraw(_drawPreviewSetting);
        }

        private void SelectPrefab(int index)
        {
            _drawPreviewSetting.IsInstanceMesh = false;
            _drawPreviewSetting.Index = index;
            Repaint();
            _editorData.TerrainEditorMgr.SetTerrainEnvironmentDraw(_drawPreviewSetting);
        }

        private void RefreshCategoryNames()
        {
            if (CurEditRuntimeData == null || CurEditRuntimeData.EnvironmentTemplateEditRuntimeData == null)
            {
                _categoryNames = new string[0];
                return;
            }
            _categoryNames = CurEditRuntimeData.EnvironmentTemplateEditRuntimeData.GetCategoryNames();
        }

        private void SelectCategory(string name)
        {
            if (_categoryNames == null)
                return;

            for (int i = 0; i < _categoryNames.Length; i++)
            {
                if (_categoryNames[i] != name)
                    continue;

                if (!CurEditRuntimeData.EnvironmentTemplateEditRuntimeData.TryGetCategory(name, out var result))
                    return;

                _curSelectedCategoryIndex = i;
                _curCategoryRuntimeData = result;
                _drawPreviewSetting.CategoryName = name;
                _editorData.TerrainEditorMgr.SetTerrainEnvironmentDraw(_drawPreviewSetting);
                return;
            }
        }

        private void SelectCategory(int index)
        {
            if (_categoryNames == null)
                return;

            var maxIndex = _categoryNames.Length - 1;
            index = Mathf.Clamp(index, Mathf.Min(0, maxIndex), maxIndex);
            if (index < 0)
                return;
            SelectCategory(_categoryNames[index]);
        }
    }
}
