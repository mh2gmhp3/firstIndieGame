using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GameSystem
{
#if UNITY_EDITOR
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

        public StepConfig(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
#endif

    [CreateAssetMenu(fileName = "GameSystemEnterGameFlowStepSetting",  menuName = "GameSystem/GameSystemEnterGameFlowStepSetting")]
    public class GameSystemEnterGameFlowStepSetting : ScriptableObject
    {
        /// <summary>
        /// 檔案路徑 TODO 盡可能拆出去
        /// </summary>
        public static string RESOURCE_FRAMEWORK_PATH = "Framework/Setting/GameSystem/GameSystemEnterGameFlowStepSetting";

        public List<int> EnterGameFlowStep = new List<int>();

#if UNITY_EDITOR
        public List<StepConfig> EnterGameFlowStepConfigs = new List<StepConfig>();

        /// <summary>
        /// 嘗試獲取階段設定Dic
        /// </summary>
        /// <param name="idToConfigDic"></param>
        /// <returns>具有相同Id時回傳false</returns>
        public bool TryGetStepConfigDic(out Dictionary<int, StepConfig> idToConfigDic)
        {
            idToConfigDic = new Dictionary<int, StepConfig>();
            foreach (var stepConfig in EnterGameFlowStepConfigs)
            {
                if (idToConfigDic.ContainsKey(stepConfig.Id))
                    return false;

                idToConfigDic.Add(stepConfig.Id, stepConfig);
            }
            return true;
        }
#endif
    }
}
