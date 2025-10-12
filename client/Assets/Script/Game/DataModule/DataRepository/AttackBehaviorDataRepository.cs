using FormModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModule
{
    public class AttackBehaviorData
    {
        public int RefItemId;
        public int SettingId;

        public AttackBehaviorData(int refItemId, int settingId)
        {
            RefItemId = refItemId;
            SettingId = settingId;
        }
    }

    public class AttackBehaviorDataContainer
    {
        public List<AttackBehaviorData> AttackBehaviorDataList = new List<AttackBehaviorData>();
    }

    [DataRepository(1)]
    public class AttackBehaviorDataRepository : DataRepository<AttackBehaviorDataContainer>
    {
        private Dictionary<int, AttackBehaviorData> _idToDataDic = new Dictionary<int, AttackBehaviorData>();

        public AttackBehaviorDataRepository(DataManager dataManager, int version) : base(dataManager, version)
        {
        }

        protected override void OnLoad(int currentVersion, int loadedVersion)
        {
            for (int i = 0; i < _data.AttackBehaviorDataList.Count; i++)
            {
                var data = _data.AttackBehaviorDataList[i];
                //整理Mapping
                if (_idToDataDic.ContainsKey(data.RefItemId))
                {
                    Log.LogError($"AttackBehaviorDataRepository OnLoad Error, 有相同Id資料! RefItemId:Id{data.RefItemId} SettingId:{data.SettingId}");
                }
                else
                {
                    _idToDataDic.Add(data.RefItemId, data);
                }
            }
        }

        /// <summary>
        /// 新增攻擊行為
        /// </summary>
        /// <param name="refItemId"></param>
        /// <param name="settingId"></param>
        public void AddBehavior(int refItemId, int settingId)
        {
            if (_idToDataDic.ContainsKey(refItemId))
            {
                Log.LogError($"AttackBehaviorDataRepository AddBehavior Error, RefItemId already exist, " +
                    $"RefItemId:{refItemId}, SettingId:{settingId}");
                return;
            }

            if (!FormSystem.Table.AttackBehaviorSettingTable.TryGetData(settingId, out _))
            {
                Log.LogError($"AttackBehaviorDataRepository AddBehavior Error, 找不到此設定資料 SettingId:{settingId}");
                return;
            }

            var data = new AttackBehaviorData(refItemId, settingId);
            _data.AttackBehaviorDataList.Add(data);
            _idToDataDic.Add(refItemId, data);
        }

        /// <summary>
        /// 移除攻擊行為
        /// </summary>
        /// <param name="refItemId"></param>
        public void RemoveBehavior(int refItemId)
        {
            if (!_idToDataDic.TryGetValue(refItemId, out var data))
            {
                Log.LogError($"AttackBehaviorDataRepository RemoveBehavior Error, RefItemId not exist, " +
                    $"RefItemId:{refItemId}");
                return;
            }

            _idToDataDic.Remove(refItemId);
            _data.AttackBehaviorDataList.Remove(data);
        }

        public List<AttackBehaviorData> GetAttackBehaviorDataList() { return _data.AttackBehaviorDataList; }
    }
}
