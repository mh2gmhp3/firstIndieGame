using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GameSystem
{
    /// <summary>
    /// 階段設定
    /// </summary>
    [Serializable]
    public class StepConfig
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id;
        /// <summary>
        /// 階段名稱
        /// </summary>
        public string Name;
        /// <summary>
        /// 於一般模式下啟用
        /// </summary>
        public bool EnableInNormalMode = true;
        /// <summary>
        /// 於測試模式時啟用
        /// </summary>
        public bool EnableInTestMode = true;

        public StepConfig(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    [CreateAssetMenu(fileName = "GameSystemEnterGameFlowStepSetting",  menuName = "GameSystem/GameSystemEnterGameFlowStepSetting")]
    public class GameSystemEnterGameFlowStepSetting : ScriptableObject
    {
        /// <summary>
        /// 檔案路徑 TODO 盡可能拆出去
        /// </summary>
        public static string RESOURCE_FRAMEWORK_PATH = "Framework/Setting/GameSystem/GameSystemEnterGameFlowStepSetting";

        public List<StepConfig> EnterGameFlowStepConfigs = new List<StepConfig>();
    }
}
