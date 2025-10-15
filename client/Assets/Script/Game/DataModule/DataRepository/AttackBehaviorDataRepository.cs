using FormModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UIModule;
using UnityEngine;

namespace DataModule
{
    #region Runtime

    public class AttackBehaviorData : IUIData
    {
        public int RefItemId;
        public int SettingId;

        public AttackBehaviorData(AttackBehaviorRepoData data)
        {
            SyncData(data);
        }

        public void SyncData(AttackBehaviorRepoData data)
        {
            RefItemId = data.RefItemId;
            SettingId = data.SettingId;
        }
    }

    #endregion

    public class AttackBehaviorRepoData
    {
        public int RefItemId;
        public int SettingId;

        public AttackBehaviorRepoData(int refItemId, int settingId)
        {
            RefItemId = refItemId;
            SettingId = settingId;
        }
    }

    public class AttackBehaviorRepoDataContainer
    {
        public List<AttackBehaviorRepoData> AttackBehaviorDataList = new List<AttackBehaviorRepoData>();
    }

    [DataRepository(1)]
    public class AttackBehaviorDataRepository : DataRepository<AttackBehaviorRepoDataContainer>
    {
        private Dictionary<int, AttackBehaviorRepoData> _idToRepoDataDic = new Dictionary<int, AttackBehaviorRepoData>();

        //Runtime
        private List<AttackBehaviorData> _runtimeDataList = new List<AttackBehaviorData>();
        private Dictionary<int, AttackBehaviorData> _idToRuntimeDataDic = new Dictionary<int, AttackBehaviorData>();

        public AttackBehaviorDataRepository(DataManager dataManager, int version) : base(dataManager, version)
        {
        }

        protected override void OnLoad(int currentVersion, int loadedVersion)
        {
            for (int i = 0; i < _data.AttackBehaviorDataList.Count; i++)
            {
                var data = _data.AttackBehaviorDataList[i];
                //整理Mapping
                if (_idToRepoDataDic.ContainsKey(data.RefItemId))
                {
                    Log.LogError($"AttackBehaviorDataRepository OnLoad Error, 有相同Id資料! RefItemId:Id{data.RefItemId} SettingId:{data.SettingId}");
                }
                else
                {
                    _idToRepoDataDic.Add(data.RefItemId, data);
                    //Runtime
                    var runtimeData = new AttackBehaviorData(data);
                    _runtimeDataList.Add(runtimeData);
                    _idToRuntimeDataDic.Add(data.RefItemId, runtimeData);
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
            if (_idToRepoDataDic.ContainsKey(refItemId))
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

            var data = new AttackBehaviorRepoData(refItemId, settingId);
            _data.AttackBehaviorDataList.Add(data);
            _idToRepoDataDic.Add(refItemId, data);
            //Runtime
            var runtimeData = new AttackBehaviorData(data);
            _runtimeDataList.Add(runtimeData);
            _idToRuntimeDataDic.Add(runtimeData.RefItemId, runtimeData);
        }

        /// <summary>
        /// 移除攻擊行為
        /// </summary>
        /// <param name="refItemId"></param>
        public void RemoveBehavior(int refItemId)
        {
            if (!_idToRepoDataDic.TryGetValue(refItemId, out var data))
            {
                Log.LogError($"AttackBehaviorDataRepository RemoveBehavior Error, RefItemId not exist, " +
                    $"RefItemId:{refItemId}");
                return;
            }

            _idToRepoDataDic.Remove(refItemId);
            _data.AttackBehaviorDataList.Remove(data);
            //Runtime
            if (_idToRuntimeDataDic.TryGetValue(refItemId, out var runtimeData))
            {
                _runtimeDataList.Remove(runtimeData);
                _idToRuntimeDataDic.Remove(refItemId);
            }
        }

        public List<AttackBehaviorData> GetAttackBehaviorDataList() { return _runtimeDataList; }

        public bool TryGetAttackBehaviorData(int behaviorRefItemId, out AttackBehaviorData result)
        {
            return _idToRuntimeDataDic.TryGetValue(behaviorRefItemId, out result);
        }
    }
}
