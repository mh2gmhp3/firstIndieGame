using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UEditor = UnityEditor.Editor;
using Framework.Logging;
using Framework.GameSystem;
using System;

namespace Framework.Editor.GameSystem.EnterGameFlowStepSetting
{
    [CustomEditor(typeof(GameSystemEnterGameFlowStepSetting))]
    public class GameSystemEnterGameFlowStepSettingEditor : UEditor
    {
        private GameSystemEnterGameFlowStepSetting _instance = null;

        private HashSet<string> _stepConfigName = new HashSet<string>();
        private Dictionary<int, StepConfig> _idToStepCongifDic = new Dictionary<int, StepConfig>();
        private string _newStepConfigName = string.Empty;

        private void OnEnable()
        {
            _instance = target as GameSystemEnterGameFlowStepSetting;
            InitEdiotrSetting();
        }

        public override void OnInspectorGUI()
        {
            if (_instance == null)
            {
                Log.LogError("GameSystemEnterGameFlowStepSettingEditor _instance is null");
                return;
            }

            DrawStepGUI();
            EditorGUILayout.Space(10);
            DrawStepConfigGUI();
        }

        private void InitEdiotrSetting()
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

            _instance.TryGetStepConfigDic(out _idToStepCongifDic);
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

        List<int> _removeStepList = new List<int>();
        private void DrawStepGUI()
        {
            for (int i = 0; i < _instance.EnterGameFlowStep.Count; i++)
            {
                var stepId= _instance.EnterGameFlowStep[i];
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    GUILayout.Label($"{stepId}", GUILayout.Width(50));
                    if (_idToStepCongifDic.TryGetValue(stepId, out var stepConfig))
                    {
                        GUILayout.Label(stepConfig.Name);
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("X"))
                    {
                        _removeStepList.Add(stepId);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            foreach (var removeStep in _removeStepList)
            {
                _instance.EnterGameFlowStep.Remove(removeStep);
            }
            if (_removeStepList.Count > 0)
                _removeStepList.Clear();
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
                    GUILayout.Label($"{stepConfig.Id}", GUILayout.Width(50));
                    GUILayout.Label(stepConfig.Name);
                    GUILayout.FlexibleSpace();
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
    }
}
