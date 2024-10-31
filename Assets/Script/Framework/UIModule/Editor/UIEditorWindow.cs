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
using static PlasticPipe.PlasticProtocol.Messages.Serialization.ItemHandlerMessagesSerialization;
using static UnityEngine.GraphicsBuffer;

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

        #endregion

        #region Setting 應該要抽出去

        private string UI_SCRIPT_TAMPLATE_FILE_MAIN_PATH = "Assets/Script/Framework/UIModule/Editor/ScriptTemplate/UIBaseTemplate.txt";
        private string UI_SCRIPT_TAMPLATE_FILE_INIT_COMPONENT_PATH = "Assets/Script/Framework/UIModule/Editor/ScriptTemplate/UIBaseTemplate.InitComponent.txt";
        private string UI_SCRIPT_INIT_COMPONENT_FILE_NAME = "InitComponent";
        private string UI_SCRIPT_FOLDER_PATH = "Assets/Script/Game/UIModule/Script";

        #endregion

        private static UIEditorWindow _instance = null;

        private GameObject _targetGameObject = null;
        private UIBase _targetUIBase = null;
        private SerializedObject _targetSerializedObject = null;

        private ObjectReferenceDatabaseEditorGUI _objRefEditorGUI = null;
        private ExpandTextField _newUIComponentName = new ExpandTextField();

        private void OnEnable()
        {
            _instance = this;
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
                    if (_newUIComponentName.DrawButtonIfExpand("新增"))
                    {
                        if (AddNewUICompnent(_newUIComponentName.Text))
                        {
                            _newUIComponentName.Reset();
                        }
                    }
                }
            }

            if (_targetSerializedObject != null)
            {
                Undo.RecordObject(_targetSerializedObject.targetObject, "UIWindows");
                _targetSerializedObject.ApplyModifiedProperties();
                _targetSerializedObject.Update();
            }

            if (_objRefEditorGUI != null)
            {
                _objRefEditorGUI.OnGUI();
            }
        }

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
                _targetSerializedObject = new SerializedObject(_targetUIBase);
            }
            else
            {
                _targetUIBase = null;
                _targetSerializedObject = null;
            }

            if (_targetUIBase != null)
                _objRefEditorGUI = new ObjectReferenceDatabaseEditorGUI(_targetUIBase.ObjectReferenceDb);
            else
                _objRefEditorGUI = null;
        }

        /// <summary>
        /// 清空編輯目標
        /// </summary>
        private void ClearEditTarget()
        {
            _targetGameObject = null;
            _targetUIBase = null;
            _objRefEditorGUI = null;
        }

        #endregion

        #region Add New UI

        /// <summary>
        /// 新增新的UIComponent
        /// </summary>
        /// <param name="newAddComonentName"></param>
        /// <returns></returns>
        private bool AddNewUICompnent(string newAddComonentName)
        {
            if (string.IsNullOrEmpty(newAddComonentName))
                return false;

            string newAddComponentFilePath = Path.Combine(
                UI_SCRIPT_FOLDER_PATH,
                $"{newAddComonentName}.cs");
            string newAddComponentInitCompFilePath = Path.Combine(
                UI_SCRIPT_FOLDER_PATH,
                $"{newAddComonentName}.{UI_SCRIPT_INIT_COMPONENT_FILE_NAME}.cs");

            List<TemplateReplaceText> templateReplaceTexts
                = new List<TemplateReplaceText>
                {
                    new TemplateReplaceText
                    {
                        Mark = "#SCRIPT_NAME#",
                        Text = newAddComonentName
                    }
                };

            var scriptObj = ScriptGenerator.GenScript(
                UI_SCRIPT_TAMPLATE_FILE_MAIN_PATH,
                newAddComponentFilePath,
                templateReplaceTexts);

            ScriptGenerator.GenScript(
                UI_SCRIPT_TAMPLATE_FILE_INIT_COMPONENT_PATH,
                newAddComponentInitCompFilePath,
                templateReplaceTexts);

            UnityEditorInternalExtension.AddComponentUncheckedUndoable(_targetGameObject, scriptObj);

            return true;
        }

        #endregion
    }
}
