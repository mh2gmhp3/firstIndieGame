using Framework.Editor;
using Framework.Editor.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class EnvironmentTemplateEditDataPage : EnvironmentTemplateBasePage
    {
        public override string Name => TerrainEditorDefine.EnvironmentTemplateEditPageToName[(int)EnvironmentTemplateEditPageType.Edit];

        private EnvironmentTemplateEditRuntimeData CurEditRuntimeData => _editorData.CurEnvironmentTemplateEditRuntimeData;
        private EnvironmentTemplateCategoryEditRuntimeData _curCategoryRuntimeData;

        private string[] _categoryNames;
        private int _curSelectedCategoryIndex = -1;

        private string _newCategoryName;

        private GameObject _addNewInstanceMeshPrefab;
        private Vector2 _instanceMeshScrollPos = Vector2.zero;
        private int _removeInsMeshIndex = -1;

        private GameObject _addNewPrefab;
        private Vector2 _prefabScrollPos = Vector2.zero;
        private int _removePrefabIndex = -1;

        private EnvironmentTemplatePreviewSetting _previewSetting = new EnvironmentTemplatePreviewSetting();

        public EnvironmentTemplateEditDataPage(TerrainEditorData editorData) : base(editorData)
        {
            _editorData.EnvironmentTemplatePreviewSetting = _previewSetting;
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
            _curCategoryRuntimeData = null;
        }

        public override void OnGUI()
        {
            if (CurEditRuntimeData == null)
                return;

            EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
            {
                DrawCurEditData();
                GUILayout.Space(5f);
                DrawGUI_Category();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            DrawGUI_CategoryData();
        }

        private void DrawCurEditData()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box, GUILayout.MaxWidth(_editorData.EditorWindow.position.width / 2));
            {
                EditorGUILayout.TextField("名稱", CurEditRuntimeData.Name);
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("存檔"))
                    {
                        _editorData.SaveCurEnvironmentTemplateData();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGUI_Category()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    _newCategoryName = EditorGUILayout.TextField("名稱:", _newCategoryName);

                    if (GUILayout.Button("新增分類"))
                    {
                        if (CreateNewCategory(_newCategoryName))
                        {
                            _newCategoryName = string.Empty;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

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

                    if (GUILayout.Button("刷新分類"))
                    {
                        RefreshCategoryNames();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGUI_CategoryData()
        {
            if (_curCategoryRuntimeData == null)
                return;

            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("刪除分類"))
                    {
                        if (CurEditRuntimeData.RemoveCategory(_curCategoryRuntimeData.CategoryName))
                        {
                            _curCategoryRuntimeData = null;
                            RefreshCategoryNames();
                            SelectCategory(_curSelectedCategoryIndex);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5f);
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
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    _addNewInstanceMeshPrefab = EditorGUILayout.ObjectField(_addNewInstanceMeshPrefab, typeof(GameObject), false) as GameObject;
                    if (GUILayout.Button("新增"))
                    {
                        var result = CurEditRuntimeData.AddInstanceMesh(_curCategoryRuntimeData.CategoryName, _addNewInstanceMeshPrefab);
                        var msg = string.Empty;
                        switch (result)
                        {
                            case AddInstanceMeshResult.ObjectIsNull:
                                msg = "物件為空";
                                break;
                            case AddInstanceMeshResult.CategoryNotFound:
                                msg = $"無法找到分類名稱 {_curCategoryRuntimeData.CategoryName}";
                                break;
                            case AddInstanceMeshResult.DuplicateName:
                                msg = "不允許有重複名稱物件";
                                break;
                            case AddInstanceMeshResult.DoNotHaveAnyMesh:
                                msg = "此物件不包含任何Mesh";
                                break;
                        }
                        if (!string.IsNullOrEmpty(msg))
                        {
                            EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, msg, TerrainEditorDefine.Dialog_Ok_Confirm);
                        }
                        else
                        {
                            SelectInstanceMesh(_curCategoryRuntimeData.InstanceMeshDataList.Count - 1);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

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
            if (_removeInsMeshIndex > -1)
            {
                _curCategoryRuntimeData.InstanceMeshDataList.RemoveAt(_removeInsMeshIndex);
                _removeInsMeshIndex = -1;
            }
        }

        private void DrawGUI_InstanceMeshCell(EnvironmentTemplateInstanceMeshEditRuntimeData data, int index)
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.SelectableBlueBox(
                _previewSetting.IsInstanceMesh && _previewSetting.Index == index),
                GUILayout.Width(160), GUILayout.MaxWidth(160));
            {
                EditorGUILayout.ObjectField(data.OriginObject.EditorInstance, typeof(GameObject), false);
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    var rect = GUILayoutUtility.GetRect(150, 150, GUILayout.Width(150), GUILayout.Height(150));
                    TerrainEditorUtility.DrawEnvironmentTemplatePreview(data, rect);
                }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("X", GUILayout.MaxWidth(50)))
                {
                    _removeInsMeshIndex = index;
                }
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
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    _addNewPrefab = EditorGUILayout.ObjectField(_addNewPrefab, typeof(GameObject), false) as GameObject;
                    if (GUILayout.Button("新增"))
                    {
                        var result = CurEditRuntimeData.AddPrefab(_curCategoryRuntimeData.CategoryName, _addNewPrefab);
                        var msg = string.Empty;
                        switch (result)
                        {
                            case AddPrefabResult.ObjectIsNull:
                                msg = "物件為空";
                                break;
                            case AddPrefabResult.CategoryNotFound:
                                msg = $"無法找到分類名稱 {_curCategoryRuntimeData.CategoryName}";
                                break;
                            case AddPrefabResult.DuplicateName:
                                msg = "不允許有重複名稱物件";
                                break;
                        }
                        if (!string.IsNullOrEmpty(msg))
                        {
                            EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, msg, TerrainEditorDefine.Dialog_Ok_Confirm);
                        }
                        else
                        {
                            SelectPrefab(_curCategoryRuntimeData.PrefabList.Count - 1);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

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
            if (_removePrefabIndex > -1)
            {
                _curCategoryRuntimeData.PrefabList.RemoveAt(_removePrefabIndex);
                _removePrefabIndex = -1;
            }
        }

        private void DrawGUI_PrefabCell(EnvironmentTemplatePrefabEditRuntimeData data, int index)
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.SelectableBlueBox(
                !_previewSetting.IsInstanceMesh && _previewSetting.Index == index),
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
                if (GUILayout.Button("X", GUILayout.MaxWidth(50)))
                {
                    _removePrefabIndex = index;
                }
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
            _previewSetting.IsInstanceMesh = true;
            _previewSetting.Index = index;
            Repaint();
            _editorData.TerrainEditorMgr.RefreshEnvironmentTemplatePreview();
        }

        private void SelectPrefab(int index)
        {
            _previewSetting.IsInstanceMesh = false;
            _previewSetting.Index = index;
            Repaint();
            _editorData.TerrainEditorMgr.RefreshEnvironmentTemplatePreview();
        }

        private bool CreateNewCategory(string name)
        {
            var result = CurEditRuntimeData.AddCategory(name);
            if (result == EnvironmentTemplateEditRuntimeData.AddCategoryResult.NameIsEmpty)
            {
                EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, "名稱不能為空白", TerrainEditorDefine.Dialog_Ok_Confirm);
                return false;
            }
            else if (result == EnvironmentTemplateEditRuntimeData.AddCategoryResult.NameDuplicate)
            {
                EditorUtility.DisplayDialog(TerrainEditorDefine.Dialog_Title_Error, "已有相同名稱", TerrainEditorDefine.Dialog_Ok_Confirm);
                return false;
            }

            RefreshCategoryNames();
            SelectCategory(name);
            return true;
        }

        private void RefreshCategoryNames()
        {
            if (CurEditRuntimeData == null)
            {
                _categoryNames = new string[0];
                return;
            }
            _categoryNames = CurEditRuntimeData.GetCategoryNames();
        }

        private void SelectCategory(string name)
        {
            if (_categoryNames == null)
                return;

            for (int i = 0; i < _categoryNames.Length; i++)
            {
                if (_categoryNames[i] != name)
                    continue;

                if (!CurEditRuntimeData.TryGetCategory(name, out var result))
                    return;

                _curSelectedCategoryIndex = i;
                _curCategoryRuntimeData = result;
                _previewSetting.CategoryName = name;
                _editorData.TerrainEditorMgr.RefreshEnvironmentTemplatePreview();
                return;
            }
        }

        private void SelectCategory(int index)
        {
            if (_categoryNames == null)
                return;

            var maxIndex = _categoryNames.Length - 1;
            index = Mathf.Clamp(index,  Mathf.Min(0, maxIndex), maxIndex);
            if (index < 0)
                return;
            SelectCategory(_categoryNames[index]);
        }
    }
}
