using Framework.Editor;
using Framework.Editor.Utility;
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
        private EnvironmentTemplateCategoryRuntimeData _curCategoryRuntimeData;

        private string[] _categoryNames;
        private int _curSelectedCategoryIndex = -1;

        private string _newCategoryName;

        public EnvironmentTemplateEditDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override void OnEnable()
        {
            RefreshCategoryNames();
            if (_curSelectedCategoryIndex == -1)
                _curSelectedCategoryIndex = _categoryNames.Length - 1;
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

            DrawGUI_Category();
            GUILayout.Space(5f);
            DrawGUI_CategoryData();
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
                    if (_categoryNames != null || _categoryNames.Length > 0)
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
                DrawGUI_InstanceMesh();
                GUILayout.Space(5f);
                DrawGUI_Prefab();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGUI_InstanceMesh()
        {

        }

        private void DrawGUI_Prefab()
        {

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
