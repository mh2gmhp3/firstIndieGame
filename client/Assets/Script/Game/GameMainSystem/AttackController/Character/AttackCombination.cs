using FormModule;
using Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Attack
{
    [Serializable]
    public class AttackCombination
    {
        public int WeaponGroup = 0;
        public int WeaponSettingId = 0;

        public List<AttackBehaviorInfo> MainAttackBehaviorList = new List<AttackBehaviorInfo>();
        public List<AttackBehaviorInfo> SubAttackBehaviorList = new List<AttackBehaviorInfo>();

        public Dictionary<string, AnimationClip> AllAttackClipCache = new Dictionary<string, AnimationClip>();

        public void Update(AttackCombinationRuntimeSetupData setupData)
        {
            WeaponGroup = setupData.Group;
            WeaponSettingId = setupData.WeaponSettingId;
            MainAttackBehaviorList.Clear();
            //sub先不使用
            SubAttackBehaviorList.Clear();
            AllAttackClipCache.Clear();
            for (int i = 0; i < setupData.BehaviorRuntimeSetupDataList.Count; i++)
            {
                var setup = setupData.BehaviorRuntimeSetupDataList[i];
                var settingId = setup.SettingId;
                if (!FormSystem.Table.AttackBehaviorSettingTable.TryGetData(settingId, out var behaviorRow))
                {
                    Log.LogError($"AttackCombination Update Error, behaviorRow not found, Id:{settingId}");
                    continue;
                }

                if (!GameMainSystem.AttackBehaviorAssetSetting.TryGetRuntimeAsset(behaviorRow.AssetSettingId, out var assetSetting))
                {
                    Log.LogError($"AttackCombination Update Error, behaviorAssetSetting not found, Id:{behaviorRow.AssetSettingId}");
                    continue;
                }

                for (int j = 0; j < assetSetting.AttackClipTrackList.Count; j++)
                {
                    var clip = assetSetting.AttackClipTrackList[j].Clip;
                    if (clip == null)
                        continue;
                    if (AllAttackClipCache.ContainsKey(clip.name))
                        continue;
                    AllAttackClipCache.Add(clip.name, clip);
                }

                MainAttackBehaviorList.Add(new AttackBehaviorInfo(setup.RefItemId, assetSetting));
            }

            Log.LogInfo($"AttackCombination Update Weapon:{WeaponGroup} Behavior:{MainAttackBehaviorList}");
        }
    }
}
