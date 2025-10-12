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
    }

    [CreateAssetMenu(fileName = "AttackBehaviorAssetSetting", menuName = "GameMainModule/Attack/AttackBehaviorAssetSetting")]
    public class AttackBehaviorAssetSetting : ScriptableObject
    {
        public List<AttackBehaviorAssetSettingGroupData> GroupDataList = new List<AttackBehaviorAssetSettingGroupData>();
        public List<AttackBehaviorAssetSettingData> SettingList =
            new List<AttackBehaviorAssetSettingData>();
        public List<AttackBehaviorAnimationOverrideGroupSetting> AnimationOverrideGroupSettingList =
            new List<AttackBehaviorAnimationOverrideGroupSetting>();

#if UNITY_EDITOR
        [MenuItem("Assets/UIModule/CopyList")]
        public static void CopyToList()
        {
            if (Selection.activeObject is AttackBehaviorAssetSetting asset)
            {
                asset.SettingList.Clear();
                asset.AnimationOverrideGroupSettingList.Clear();
                for (int i = 0; i < asset.GroupDataList.Count; i++)
                {
                    var overrideData = new AttackBehaviorAnimationOverrideGroupSetting();
                    overrideData.GroupId = asset.GroupDataList[i].Group;
                    for (int j = 0; j < asset.GroupDataList[i].DataList.Count; j++)
                    {
                        var data = asset.GroupDataList[i].DataList[j];
                        asset.SettingList.Add(data);
                        overrideData.OverrideSettingList.Add(new AttackBehaviorAnimationOverrideSetting()
                        {
                            AnimationName = data.AnimationClipOverrideNameAndStateName,
                            AnimationOverrideClip = data.AnimationClip
                        });
                    }
                    asset.AnimationOverrideGroupSettingList.Add(overrideData);
                }

                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }
#endif

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
    }
}
