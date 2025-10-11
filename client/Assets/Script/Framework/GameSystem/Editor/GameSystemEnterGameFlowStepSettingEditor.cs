using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UEditor = UnityEditor.Editor;
using Logging;
using GameSystem;
using System;
using Extension;

namespace Framework.Editor.GameSystem.EnterGameFlowStepSetting
{
    [CustomEditor(typeof(GameSystemEnterGameFlowStepSetting))]
    public class GameSystemEnterGameFlowStepSettingEditor : UEditor
    {
        public GUISkin _guiSkin;

        private GameSystemEnterGameFlowStepSetting _instance = null;

        private HashSet<string> _stepConfigName = new HashSet<string>();
        private string _newStepConfigName = string.Empty;

        private void OnEnable()
        {
            _instance = target as GameSystemEnterGameFlowStepSetting;
            InitEditorSetting();
        }

        public override void OnInspectorGUI()
        {
            if (_instance == null)
            {
                Log.LogError("GameSystemEnterGameFlowStepSettingEditor _instance is null");
                return;
            }

            Undo.RecordObject(
                _instance,
                "GameSystemEnterGameFlowStepSetting");

            DrawStepConfigGUI();

            DrawSaveGUI();

            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        private void InitEditorSetting()
        {
            RefreshStepConfigSetting();
        }

        private void RefreshStepConfigSetting()
        {
            if (_instance == null)
                return;

            _stepConfigName.Clear();
            for (int i = 0; i < _instance.EnterGameFlowStepConfigs.Count; i++)
            {
                _stepConfigName.Add(_instance.EnterGameFlowStepConfigs[i].Name);
            }
        }

        private void AddNewStepConfig(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            name = name.Replace(" ", "");
            if (_stepConfigName.Contains(name))
                return;

            if (!TryGetMaxStepConfigId(out int newId))
                return;

            _instance.EnterGameFlowStepConfigs.Add(
                new StepConfig(newId, name));

            EditorUtility.SetDirty(_instance);

            RefreshStepConfigSetting();
        }

        private bool TryGetMaxStepConfigId(out int id)
        {
            id = 0;
            if (_instance == null)
                return false; ;

            id = 0;
            for (int i = 0; i < _instance.EnterGameFlowStepConfigs.Count; i++)
            {
                int oriId = _instance.EnterGameFlowStepConfigs[i].Id;
                if (oriId > id)
                    id = oriId;
            }

            id++;
            return true;
        }

        #region DrawStepConfigGUI

        private void DrawStepConfigGUI()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                DrawStepConfig_NewStepConfigGUI();

                EditorGUILayout.Space(10);

                DrawStepConfig_StepConfigListGUI();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawStepConfig_NewStepConfigGUI()
        {
            EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
            {
                GUILayout.Label("新增階段名稱");
                _newStepConfigName = GUILayout.TextField(_newStepConfigName);
                if (GUILayout.Button("新增"))
                {
                    AddNewStepConfig(_newStepConfigName);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        List<StepConfig> _removeStepConfigList = new List<StepConfig>();
        private void DrawStepConfig_StepConfigListGUI()
        {
            for (int i = 0; i < _instance.EnterGameFlowStepConfigs.Count; i++)
            {
                var stepConfig = _instance.EnterGameFlowStepConfigs[i];
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    var id = stepConfig.Id;
                    EditorGUI.BeginChangeCheck();
                    id = EditorGUILayout.IntField(id, GUILayout.Width(50));
                    if (EditorGUI.EndChangeCheck())
                    {
                        var res = _instance.EnterGameFlowStepConfigs.Find(s => s.Id == id);
                        if (res == null)
                        {
                            stepConfig.Id = id;
                            RefreshStepConfigSetting();
                        }
                    }
                    GUILayout.Label(stepConfig.Name);
                    GUILayout.FlexibleSpace();
                    stepConfig.EnableInNormalMode = EditorGUILayout.ToggleLeft("Normal", stepConfig.EnableInNormalMode, GUILayout.Width(100));
                    stepConfig.EnableInTestMode = EditorGUILayout.ToggleLeft("Test", stepConfig.EnableInTestMode, GUILayout.Width(100));
                    if (GUILayout.Button("+"))
                    {
                        _instance.EnterGameFlowStepConfigs.MoveToPrevious(i);
                    }
                    if (GUILayout.Button("-"))
                    {
                        _instance.EnterGameFlowStepConfigs.MoveToNext(i);
                    }
                    if (GUILayout.Button("X"))
                    {
                        _removeStepConfigList.Add(stepConfig);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            foreach (var removeStepConfig in _removeStepConfigList)
            {
                _instance.EnterGameFlowStepConfigs.Remove(removeStepConfig);
            }
            if (_removeStepConfigList.Count > 0)
            {
                RefreshStepConfigSetting();
                _removeStepConfigList.Clear();
            }
        }

        #endregion

        #region DrawSaveGUI

        private void DrawSaveGUI()
        {
            if (GUILayout.Button("保存"))
            {
                EditorUtility.SetDirty(_instance);
                AssetDatabase.SaveAssets();
            }
        }

        #endregion
    }
}
