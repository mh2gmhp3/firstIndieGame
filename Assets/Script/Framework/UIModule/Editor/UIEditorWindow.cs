using Framework.ComponentUtility.Editor;
using Framework.Editor;
using Framework.Editor.ScriptGenerator;
using Framework.Editor.Utility;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Utility;
using static UIModule.UIEditorDefine;

namespace UIModule
{
    public class UIEditorWindow : EditorWindow
    {
        #region Text Const

        private const string WARINNING_TITLE = "警告";

        private const string SET_TARGET_IS_NULL = "設定的編輯目標是null 設定失敗";

        private const string OK = "是";

        #endregion

        #region OpenWindow

        [MenuItem("Assets/UIModule/UIEdiotrWindow")]
        [MenuItem("GameObject/UIModule/UIEdiotrWindow")]
        public static void OpenWindow()
        {
            var window = GetWindow<UIEditorWindow>();
            window.SetEditTarget(Selection.activeGameObject);
            _instance = window;
        }

        public static void OpenWindow(GameObject editGo)
        {
            var window = GetWindow<UIEditorWindow>();
            window.SetEditTarget(editGo);
            _instance = window;
        }


        #endregion

        private static UIEditorWindow _instance = null;

        private GameObject _targetGameObject = null;
        private UIBase _targetUIBase = null;

        private UIBaseEditorGUI _uiBaseEditorGUI = null;
        private ExpandTextField _newUIComponentName = new ExpandTextField();
        private int _newUIComponentTypeIndex = 0;

        private string[] _uiTypeNames = null;

        private void OnEnable()
        {
            _instance = this;
            InitUIType();
            if (_targetGameObject != null)
                SetEditTarget(_targetGameObject);
        }

        private void OnDisable()
        {
            _instance = null;
        }

        private void OnGUI()
        {
            if (_targetGameObject == null)
            {
                if (GUILayout.Button("重新獲取編輯目標"))
                {
                    SetEditTarget(Selection.activeGameObject);
                }
            }

            if (_targetUIBase == null)
            {
                using (new EditorGUILayout.HorizontalScope(CommonGUIStyle.Default_Box))
                {
                    if (GUILayout.Button("重新獲取編輯Component"))
                    {
                        SetEditTarget(_targetGameObject);
                    }

                    if (GUILayout.Button("添加Component"))
                    {
                        _newUIComponentName.TriggerExpand();
                    }
                }

                using (new EditorGUILayout.HorizontalScope(CommonGUIStyle.Default_Box))
                {
                    _newUIComponentName.DrawTextFieldIfExpand();
                    if (_newUIComponentName.Expand)
                    {
                        _newUIComponentTypeIndex = EditorGUILayout.Popup(_newUIComponentTypeIndex, _uiTypeNames);
                    }
                    if (_newUIComponentName.DrawButtonIfExpand("新增"))
                    {
                        string uiTypeName = _uiTypeNames.Length > 0 ?
                            _uiTypeNames[_newUIComponentTypeIndex] :
                            string.Empty;
                        if (AddNewUIComponent(_newUIComponentName.Text, uiTypeName))
                        {
                            _newUIComponentName.Reset();
                        }
                    }
                }
            }

            if (_uiBaseEditorGUI != null)
            {
                _uiBaseEditorGUI.OnGUI();
            }
        }

        #region UIType

        private void InitUIType()
        {
            var uiTypeList = TypeUtility.GetTypeListByInheritsType<UIBase>(false);
            _uiTypeNames = new string[uiTypeList.Count];
            for (int i = 0; i < uiTypeList.Count; i++)
            {
                _uiTypeNames[i] = uiTypeList[i].Name;
            }
        }

        #endregion

        #region EditTarget

        /// <summary>
        /// 設定編輯目標
        /// </summary>
        /// <param name="targetGo"></param>
        private void SetEditTarget(GameObject targetGo)
        {
            if (targetGo == null)
            {
                EditorUtility.DisplayDialog(
                    WARINNING_TITLE,
                    SET_TARGET_IS_NULL,
                    OK);
                return;
            }

            _targetGameObject = targetGo;

            var uiBase = targetGo.GetComponent<UIBase>();
            if (uiBase != null && uiBase != _targetUIBase)
            {
                _targetUIBase = uiBase;
                _uiBaseEditorGUI = new UIBaseEditorGUI(_targetUIBase);
            }
            else
            {
                _targetUIBase = null;
                _uiBaseEditorGUI = null;
            }
        }

        /// <summary>
        /// 清空編輯目標
        /// </summary>
        private void ClearEditTarget()
        {
            _targetGameObject = null;
            _targetUIBase = null;
            _uiBaseEditorGUI = null;
        }

        #endregion

        #region Add New UI

        /// <summary>
        /// 新增新的UIComponent
        /// </summary>
        /// <param name="newAddComponentName"></param>
        /// <returns></returns>
        private bool AddNewUIComponent(string newAddComponentName, string uiTypeName)
        {
            if (string.IsNullOrEmpty(newAddComponentName) ||
                string.IsNullOrEmpty(uiTypeName))
                return false;

            string newAddComponentFilePath = GetUIScriptPath(
                uiTypeName,
                newAddComponentName);
            string newAddComponentInitCompFilePath = GetUIScriptInitComponentPath(
                uiTypeName,
                newAddComponentName);

            List<ReplaceText> templateReplaceTexts
                = new List<ReplaceText>
                {
                    new ReplaceText("#SCRIPT_NAME#", newAddComponentName)
                };

            var templateFileName = string.Format(UI_SCRIPT_TAMPLATE_FILE_NAME_FORMAT, uiTypeName);

            var templateMainPath = Path.Combine(
                UI_SCRIPT_TAMPLATE_FILE_MAIN_PATH,
                $"{templateFileName}.txt");
            var templateInitCompPath = Path.Combine(
                UI_SCRIPT_TAMPLATE_FILE_MAIN_PATH,
                $"{templateFileName}.{UI_SCRIPT_INIT_COMPONENT_FILE_NAME}.txt");

            var scriptObj = ScriptGenerator.GenScript(
                templateMainPath,
                newAddComponentFilePath,
                templateReplaceTexts);

            ScriptGenerator.GenScript(
                templateInitCompPath,
                newAddComponentInitCompFilePath,
                templateReplaceTexts);

            UnityEditorInternalExtension.AddComponentUncheckedUndoable(_targetGameObject, scriptObj);

            return true;
        }

        #endregion
    }
}
