using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Animation
{
    [Serializable]
    public class AnimatorTransitionSettingData
    {
        public string StateName;
        public float NormalizedTransitionDuration = 0f;
        public int Layer = -1;
        public float NormalizedTimeOffset = 0f;
        public float NormalizedTransitionTime = 0f;
    }

    [CreateAssetMenu(fileName = "AnimatorTransitionSetting", menuName = "GameMainModule/Animation/AnimatorTransitionSetting")]
    public class AnimatorTransitionSetting : ScriptableObject
    {
        public List<AnimatorTransitionSettingData> SettingDataList = new List<AnimatorTransitionSettingData>();

        public bool TryGet(string stateName, out AnimatorTransitionSettingData result)
        {
            result = null;
            for (int i = 0; i < SettingDataList.Count; i++)
            {
                var settingData = SettingDataList[i];
                if (settingData.StateName != stateName)
                    continue;

                result = settingData;
                return true;
            }
            return false;
        }
    }
}
