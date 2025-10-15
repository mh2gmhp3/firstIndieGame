using DataModule;
using FormModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Attack
{

    public struct AttackBehaviorRuntimeSetupData
    {
        public int SettingId;
        public AttackBehaviorRuntimeSetupData(int settingId)
        {
            SettingId = settingId;
        }
    }

    /// <summary>
    /// 遊戲中玩家編輯資料
    /// </summary>
    public struct AttackCombinationRuntimeSetupData
    {
        public int Group;
        public List<AttackBehaviorRuntimeSetupData> BehaviorRuntimeSetupDataList;

        public AttackCombinationRuntimeSetupData(WeaponBehaviorSetupData data)
        {
            Group = 0;
            if (GameMainSystem.TryGetWeaponData(data.WeaponRefItemId, out var weaponData) &&
                FormSystem.Table.WeaponTable.TryGetData(weaponData.SettingId, out var weaponRow))
            {
                Group = weaponRow.Type;
            }

            BehaviorRuntimeSetupDataList = new List<AttackBehaviorRuntimeSetupData>();
            for (int i = 0; i < data.AttackBehaviorRefItemIdList.Count; i++)
            {
                var behaviorRefItemId = data.AttackBehaviorRefItemIdList[i];
                if (behaviorRefItemId == CommonDefine.EmptyAttackBehaviorId)
                    continue;

                if (!GameMainSystem.TryGetAttackBehaviorData(behaviorRefItemId, out var behaviorData))
                {
                    Log.LogError($"AttackCombinationRuntimeSetupData UIWeaponBehaviorSetup Error, " +
                        $"behaviorData not found ignore behavior, " +
                        $"BehaviorRefItemId:{behaviorRefItemId}");
                    continue;
                }
                if (!FormSystem.Table.AttackBehaviorSettingTable.TryGetData(behaviorData.SettingId, out var behaviorSettingRow))
                {
                    Log.LogError($"AttackCombinationRuntimeSetupData UIWeaponBehaviorSetup Error, " +
                        $"behaviorSettingRow not found ignore behavior," +
                        $"BehaviorRefItemId:{behaviorRefItemId}, " +
                        $"SettingId:{behaviorData.SettingId}");
                    continue;
                }
                BehaviorRuntimeSetupDataList.Add(new AttackBehaviorRuntimeSetupData(behaviorData.SettingId));
            }
        }
    }
}