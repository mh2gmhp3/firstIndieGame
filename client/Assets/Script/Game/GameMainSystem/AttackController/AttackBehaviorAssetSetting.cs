using System;
using System.Collections;
using System.Collections.Generic;
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

        /// <summary>
        /// 要複寫的動畫名稱 在設定上 動畫名稱與狀態名稱會設定一樣 所以共用
        /// </summary>
        public string AnimationClipOverrideNameAndStateName = string.Empty;
        public AnimationClip AnimationClip = null;

        public float LockNextTime = 0;

        #endregion
    }

    [Serializable]
    public class AttackBehaviorAssetSettingGroupData
    {
        public int Group;
        public List<AttackBehaviorAssetSettingData> DataList = new List<AttackBehaviorAssetSettingData>();
    }

    [CreateAssetMenu(fileName = "AttackBehaviorAssetSetting", menuName = "GameMainModule/Attack/AttackBehaviorAssetSetting")]
    public class AttackBehaviorAssetSetting : ScriptableObject
    {
        public List<AttackBehaviorAssetSettingGroupData> GroupDataList = new List<AttackBehaviorAssetSettingGroupData>();

        public bool TryGetGroupOverrideClip(int group, out Dictionary<string, AnimationClip> result)
        {
            result = null;
            if (!TryGetGroupData(group, out var groupData))
                return false;

            result = new Dictionary<string, AnimationClip>();
            for ( int i = 0; i < groupData.DataList.Count; i++ )
            {
                var data = groupData.DataList[i];
                result[data.AnimationClipOverrideNameAndStateName] = data.AnimationClip;
            }

            return true;
        }

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
    }
}
