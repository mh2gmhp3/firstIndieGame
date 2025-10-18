using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static CollisionModule.CollisionAreaDefine;

namespace GameMainModule.Attack
{
    [Serializable]
    public class AttackBehaviorAssetSettingData
    {
        public int Id;

        #region Collision

        public AreaType CollisionAreaType = AreaType.Test;
        public float CollisionAreaStartTime = 0;
        public float CollisionAreaDuration = 0f;

        #endregion

        #region Animation

        public string AnimationStateName = string.Empty;

        public float LockNextTime = 0;

        #endregion
    }

    [Serializable]
    public class AttackBehaviorAnimationSetting
    {
        public string AnimationName = string.Empty;
        public AnimationClip AnimationClip = null;
    }

    [Serializable]
    public class AttackBehaviorAnimationGroupSetting
    {
        public int GroupId;
        public List<AttackBehaviorAnimationSetting> SettingList = new List<AttackBehaviorAnimationSetting>();

        public float GetBehaviorBaseTime(string animationStateName)
        {
            for (int i = 0; i < SettingList.Count; i ++)
            {
                var setting = SettingList[i];
                if (setting.AnimationName == animationStateName && setting.AnimationClip != null)
                {
                    return setting.AnimationClip.length;
                }
            }

            return 0f; ;
        }
    }

    [CreateAssetMenu(fileName = "AttackBehaviorAssetSetting", menuName = "GameMainModule/Attack/AttackBehaviorAssetSetting")]
    public class AttackBehaviorAssetSetting : ScriptableObject
    {
        public List<AttackBehaviorAssetSettingData> SettingList =
            new List<AttackBehaviorAssetSettingData>();
        public List<AttackBehaviorAnimationGroupSetting> AnimationGroupSettingList =
            new List<AttackBehaviorAnimationGroupSetting>();

        public bool TryGetAnimationNameToClipDic(int groupId, out Dictionary<string, AnimationClip> result)
        {
            result = null;
            if (!TryGetAnimationGroupSetting(groupId, out var groupData))
                return false;

            result = new Dictionary<string, AnimationClip>();
            for ( int i = 0; i < groupData.SettingList.Count; i++ )
            {
                var setting = groupData.SettingList[i];
                result[setting.AnimationName] = setting.AnimationClip;
            }

            return true;
        }

        public bool TryGetAnimationGroupSetting(int groupId, out AttackBehaviorAnimationGroupSetting result)
        {
            for (int i = 0; i < AnimationGroupSettingList.Count; i++)
            {
                if (AnimationGroupSettingList[i].GroupId == groupId)
                {
                    result = AnimationGroupSettingList[i];
                    return true;
                }
            }

            result = null;
            return false;
        }

        public bool TryGetSetting(int id, out AttackBehaviorAssetSettingData result)
        {
            for (int i = 0; i < SettingList.Count;i++)
            {
                if (SettingList[i].Id == id)
                {
                    result = SettingList[i];
                    return true;
                }
            }

            result = null;
            return false;
        }

        public float GetBehaviorBaseTime(int groupId, string animationStateName)
        {
            if (!TryGetAnimationGroupSetting(groupId, out var groupData))
            {
                return 0f;
            }

            return groupData.GetBehaviorBaseTime(animationStateName);
        }
    }
}
