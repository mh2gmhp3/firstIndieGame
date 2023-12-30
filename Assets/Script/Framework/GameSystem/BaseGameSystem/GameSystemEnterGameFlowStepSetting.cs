using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Framework.GameSystem
{
    [CreateAssetMenu(fileName = "GameSystemEnterGameFlowStepSetting",  menuName = "GameSystem/GameSystemEnterGameFlowStepSetting")]
    public class GameSystemEnterGameFlowStepSetting : ScriptableObject
    {
        /// <summary>
        /// 檔案路徑 TODO 盡可能拆出去
        /// </summary>
        public static string RESOURCE_PATH = "Setting/GameSystem/GameSystemEnterGameFlowStepSetting";

        public List<int> EnterGameFlowStep = new List<int>();
    }
}
