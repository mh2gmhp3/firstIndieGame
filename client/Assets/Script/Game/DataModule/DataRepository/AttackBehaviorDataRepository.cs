using FormModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModule
{
    public class AttackBehaviorData
    {
        public int Id;
        public int SettingId;

        public AttackBehaviorData(int id, int settingId)
        {
            Id = id;
            SettingId = settingId;
        }
    }

    public class AttackBehaviorDataContainer
    {
        public int _nextId;
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
                if (_idToDataDic.ContainsKey(data.Id))
                {
                    Log.LogError($"AttackBehaviorDataRepository OnLoad Error, 有相同Id資料! Data:Id{data.Id} SettingId:{data.SettingId}");
                }
                else
                {
                    _idToDataDic.Add(data.Id, data);
                }

                //更新下一個Id 避免重複
                if (data.Id > _data._nextId)
                    _data._nextId = data.Id;
            }
        }

        private int GetNextId()
        {
            return ++_data._nextId;
        }

        public void AddData(int settingId)
        {
            if (!FormSystem.Table.AttackBehaviorSettingTable.TryGetData(settingId, out _))
            {
                Log.LogError($"AttackBehaviorDataRepository AddData Error, 找不到此設定資料 SettingId:{settingId}");
                return;
            }

            var nextId = GetNextId();
            var data = new AttackBehaviorData(nextId, settingId);
            _data.AttackBehaviorDataList.Add(data);
            _idToDataDic.Add(nextId, data);
        }

        public List<AttackBehaviorData> GetAttackBehaviorDataList() { return _data.AttackBehaviorDataList; }
    }
}
