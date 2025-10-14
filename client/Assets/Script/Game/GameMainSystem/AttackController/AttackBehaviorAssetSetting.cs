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
    public class AttackBehaviorAnimationOverrideSetting
    {
        public string AnimationName = string.Empty;
        public AnimationClip AnimationOverrideClip = null;
    }

    [Serializable]
    public class AttackBehaviorAnimationOverrideGroupSetting
    {
        public int GroupId;
        public List<AttackBehaviorAnimationOverrideSetting> OverrideSettingList = new List<AttackBehaviorAnimationOverrideSetting>();

        public float GetBehaviorBaseTime(string animationStateName)
        {
            for (int i = 0; i < OverrideSettingList.Count; i ++)
            {
                var setting = OverrideSettingList[i];
                if (setting.AnimationName == animationStateName && setting.AnimationOverrideClip != null)
                {
                    return setting.AnimationOverrideClip.length;
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
        public List<AttackBehaviorAnimationOverrideGroupSetting> AnimationOverrideGroupSettingList =
            new List<AttackBehaviorAnimationOverrideGroupSetting>();

        public bool TryGetAnimationOverrideNameToClipDic(int groupId, out Dictionary<string, AnimationClip> result)
        {
            result = null;
            if (!TryGetAnimationOverrideGroupSetting(groupId, out var groupData))
                return false;

            result = new Dictionary<string, AnimationClip>();
            for ( int i = 0; i < groupData.OverrideSettingList.Count; i++ )
            {
                var setting = groupData.OverrideSettingList[i];
                result[setting.AnimationName] = setting.AnimationOverrideClip;
            }

            return true;
        }

        /*
        public bool TryGetGroupData(int group, out AttackBehaviorAssetSettingGroupData result)
        {
            result = null;

            for (int i = 0; i < GroupDataList.Count; i++)
            {
                var groupData = GroupDataList[i];
                if (groupData.Group != group)
                    continue;

                result = groupData;
                return true; ;
            }

            return false;
        }
        */

        public bool TryGetAnimationOverrideGroupSetting(int groupId, out AttackBehaviorAnimationOverrideGroupSetting result)
        {
            for (int i = 0; i < AnimationOverrideGroupSettingList.Count; i++)
            {
                if (AnimationOverrideGroupSettingList[i].GroupId == groupId)
                {
                    result = AnimationOverrideGroupSettingList[i];
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
            if (!TryGetAnimationOverrideGroupSetting(groupId, out var groupData))
            {
                return 0f;
            }

            return groupData.GetBehaviorBaseTime(animationStateName);
        }
    }
}
